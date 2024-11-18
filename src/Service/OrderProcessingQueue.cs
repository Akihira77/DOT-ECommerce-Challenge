using System.Threading.Channels;
using ECommerce.Store;
using ECommerce.Types;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Service;

public class OrderProcessingQueue
{
    private readonly Channel<OrderJob> queue;
    private readonly IDbContextFactory<ApplicationDbContext> dbCtxFactory;

    public OrderProcessingQueue(IDbContextFactory<ApplicationDbContext> dbCtxFactory)
    {
        this.queue = Channel.CreateUnbounded<OrderJob>();
        this.dbCtxFactory = dbCtxFactory;
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
        using var dbCtx = this.dbCtxFactory.CreateDbContext();
        using var tx = dbCtx.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
        Order? order;
        try
        {
            ct.ThrowIfCancellationRequested();

            order = await dbCtx.Orders.FirstOrDefaultAsync(o => o.Id.Equals(job.orderId), ct);
            if (order is null)
            {
                throw new Exception($"Order did not found");
            }
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err.Message}");
            throw; // Log and handle appropriately
        }

        try
        {
            ct.ThrowIfCancellationRequested();

            foreach (var item in job.myCart)
            {
                order.OrderItems.Add(new OrderItem
                {
                    OrderId = job.orderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                });

                var product = await dbCtx.Products.FirstOrDefaultAsync(p => p.Id.Equals(item.ProductId), ct);
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
            var cartItems = dbCtx.CustomerCarts.Where(c => c.CustomerId.Equals(job.customerId));
            dbCtx.CustomerCarts.RemoveRange(cartItems);

            dbCtx.OrderItems.AddRange(order.OrderItems);
            await dbCtx.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch (System.Exception err)
        {
            await tx.RollbackAsync(ct);

            dbCtx.Orders.Remove(order);

            Console.WriteLine($"There are errors {err.Message}");
            throw; // Log and handle appropriately
        }
    }

}
