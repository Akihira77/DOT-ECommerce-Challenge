using ECommerce.Service.Interface;
using ECommerce.Store;
using ECommerce.Types;
using ECommerce.Util;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Service;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext ctx;
    private readonly EmailBackgroundService emailBackgroundService;

    public OrderService(
        ApplicationDbContext ctx,
        EmailBackgroundService emailBackgroundService)
    {
        this.ctx = ctx;
        this.emailBackgroundService = emailBackgroundService;
    }

    public async Task<Order> CreateOrder(
        CancellationToken ct,
        CustomerOverviewDTO c,
        IEnumerable<CustomerCart> myCart)
    {
        using var tx = this.ctx.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
        try
        {
            ct.ThrowIfCancellationRequested();

            var now = DateTime.Now;
            var o = new Order
            {
                CustomerId = c.id,
                OrderStatus = OrderStatus.WAITING_PAYMENT,
                CreatedAt = now,
                Deadline = now.AddDays(1),
                Amount = myCart.Sum(cc => cc.Product!.Price * cc.Quantity),
                Version = 1,
            };
            await this.ctx.Orders.AddAsync(o, ct);

            var productIds = myCart
                .Select(cc => cc.ProductId)
                .ToList();
            var products = await this.ctx.Products
                .FromSqlInterpolated($@"
                        SELECT * FROM Products WITH (UPDLOCK, ROWLOCK) 
                        WHERE Id IN ({string.Join(",", productIds)})"
                )
                .ToDictionaryAsync(p => p.Id, ct);

            foreach (var item in myCart)
            {
                if (!products.TryGetValue(item.ProductId, out var product))
                {
                    throw new Exception($"Product with ID {item.ProductId} not found.");
                }

                if (product.Stock < item.Quantity)
                {
                    throw new Exception($"Insufficient stock for Product ID {item.ProductId}");
                }

                product.Stock -= item.Quantity;
                o.OrderItems.Add(new OrderItem
                {
                    OrderId = o.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                });
            }

            await this.ctx.OrderItems.AddRangeAsync(o.OrderItems, ct);
            await this.ctx.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM CustomerCarts WHERE CustomerId = {c.id}", ct);
            await this.ctx.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            this.emailBackgroundService.QueueEmail(new sendEmailData(c.email, $"Your Order with id: [{o.Id}]", $"Your order has been created. Please check http://localhost:8000/orders/{o.Id}"));
            return o;
        }
        catch (System.Exception err)
        {
            await tx.RollbackAsync(ct);
            Console.WriteLine($"There are errors ${err}");
            throw;
        }
    }

    public async Task<Order?> FindMyOrderById(
        CancellationToken ct,
        int id,
        int customerId,
        bool track)
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
                .Include(o => o.OrderTransaction)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id.Equals(id) &&
                        o.CustomerId.Equals(customerId), ct);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors ${err}");
            throw;
        }
    }

    public async Task<IEnumerable<Order>> FindMyOrderHistories(
        CancellationToken ct,
        int customerId,
        OrderStatus? os,
        bool track)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var query = this.ctx.Orders.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            if (os is not null)
            {
                query = query.Where(o => o.OrderStatus.Equals(os));
            }

            return await query
                .Include(o => o.OrderTransaction)
                .Where(o => o.CustomerId.Equals(customerId))
                .ToListAsync(ct);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors ${err}");
            throw;
        }
    }

    public async Task<Order?> FindOrderById(
        CancellationToken ct,
        int id,
        bool track)
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
                .Include(o => o.Customer)
                .Include(o => o.OrderTransaction)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id.Equals(id), ct);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors ${err}");
            throw;
        }
    }

    public async Task<IEnumerable<Order>> FindOrders(
        CancellationToken ct,
        OrderStatus? os,
        bool track)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var query = this.ctx.Orders.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            if (os is not null)
            {
                query = query.Where(o => o.OrderStatus.Equals(os));
            }

            return await query
                .Include(o => o.OrderTransaction)
                .Include(o => o.Customer)
                .ToListAsync(ct);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors ${err}");
            throw;
        }
    }

    public async Task<Order> UpdateOrderStatus(
        CancellationToken ct,
        Order o,
        UpdateOrderDTO data)
    {
        try
        {
            var orderStatus = data.orderStatus.ToEnumOrThrow<OrderStatus>();
            var updatedOrder = await this
                .ctx
                .Orders
                .FromSqlInterpolated($@"
                        UPDATE Orders 
                        SET Status = {orderStatus}, Version = Version + 1 
                        OUTPUT INSERTED.*
                        WHERE OrderID = {o.Id} AND Version = {o.Version}"
                )
                .FirstOrDefaultAsync(ct);

            if (updatedOrder is null)
            {
                throw new InvalidOperationException("The order was modified by another user.");
            }

            this.emailBackgroundService.QueueEmail(new sendEmailData(o.Customer!.Email, "Order Status Change", $"Your has change the status. Please check http://localhost:8000/orders"));
            return updatedOrder;
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors ${err}");
            throw;
        }
    }
}
