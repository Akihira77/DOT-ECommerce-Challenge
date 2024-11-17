using System.Threading.Channels;
using ECommerce.Store;
using ECommerce.Types;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Service;

public class OrderProcessingQueue
{
    private readonly Channel<OrderJob> queue;
    private readonly ApplicationDbContext ctx;

    public OrderProcessingQueue(ApplicationDbContext ctx)
    {
        this.queue = Channel.CreateUnbounded<OrderJob>();
        this.ctx = ctx;
    }

    public async Task EnqueueAsync(OrderJob job, CancellationToken ct)
    {
        await this.queue.Writer.WriteAsync(job, ct);
    }

    public async Task ProcessQueueAsync(CancellationToken ct)
    {
        await foreach (var job in this.queue.Reader.ReadAllAsync(ct))
        {
            try
            {
                await this.ProcessOrderAsync(job, ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing job {job.orderId}: {ex.Message}");
                throw;
            }
        }
    }

    public async Task ProcessOrderAsync(OrderJob job, CancellationToken ct)
    {
        using var tx = this.ctx.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
        try
        {
            ct.ThrowIfCancellationRequested();

            var order = await this.ctx.Orders.FirstOrDefaultAsync(o => o.Id.Equals(job.orderId), ct);
            if (order is null)
            {
                throw new Exception($"Order did not found");
            }

            foreach (var item in job.myCart)
            {
                this.ctx.OrderItems.Add(new OrderItem
                {
                    OrderId = job.orderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                });

                var product = await this.ctx.Products.FirstOrDefaultAsync(p => p.Id.Equals(item.ProductId), ct);
                if (product is null)
                {
                    throw new Exception($"Product did not found");
                }

                if (product.Stock < item.Quantity)
                {
                    throw new Exception($"Insufficient stock for ProductId: {item.ProductId}");
                }

                product.Stock -= item.Quantity;
            }

            // Remove cart items
            var cartItems = this.ctx.CustomerCarts.Where(c => c.CustomerId.Equals(job.customerId));
            this.ctx.CustomerCarts.RemoveRange(cartItems);

            await this.ctx.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch (System.Exception err)
        {
            await tx.RollbackAsync(ct);
            Console.WriteLine($"There are errors {err.Message}");
            throw; // Log and handle appropriately
        }
    }

}
