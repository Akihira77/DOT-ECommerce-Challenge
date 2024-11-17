using ECommerce.Store;
using ECommerce.Types;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Service;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext ctx;

    public OrderService(ApplicationDbContext ctx)
    {
        this.ctx = ctx;
    }

    public async Task<Order> CreateOrder(CancellationToken ct, int customerId, CreateOrderDTO data)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var o = new Order
            {
                CustomerId = customerId,
                OrderStatus = OrderStatus.WAITING,
                CreatedAt = DateTime.Now,
            };
            this.ctx.Orders.Add(o);
            await this.ctx.SaveChangesAsync(ct);

            var queue = new OrderProcessingQueue(this.ctx);
            await queue.EnqueueAsync(new OrderJob(o.Id, customerId, data.myCart), ct);

            return o;
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors ${err}");
            throw;
        }
    }

    public async Task<Order?> FindMyOrderById(CancellationToken ct, int id, int customerId, bool track)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var query = this.ctx.Orders.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            return await query
                .Include(o => o.Transaction)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id.Equals(id) && o.CustomerId.Equals(customerId), ct);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors ${err}");
            throw;
        }
    }

    public async Task<IEnumerable<Order>> FindMyOrderHistories(CancellationToken ct, int customerId, bool track)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var query = this.ctx.Orders.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            return await query
                .Include(o => o.Transaction)
                .Where(o => o.CustomerId.Equals(customerId))
                .ToListAsync(ct);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors ${err}");
            throw;
        }
    }

    public async Task<Order?> FindOrderById(CancellationToken ct, int id, bool track)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var query = this.ctx.Orders.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            return await query
                .Include(o => o.Transaction)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id.Equals(id), ct);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors ${err}");
            throw;
        }
    }

    public async Task<IEnumerable<Order>> FindOrders(CancellationToken ct, bool track)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var query = this.ctx.Orders.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            return await query
                .Include(o => o.Transaction)
                .Include(o => o.Customer)
                .ToListAsync(ct);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors ${err}");
            throw;
        }
    }
}
