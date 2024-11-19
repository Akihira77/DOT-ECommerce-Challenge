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

    public async Task<Order> CreateOrder(
        CancellationToken ct,
        int customerId,
        IEnumerable<CustomerCart> myCart)
    {
        using var tx = this.ctx.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
        try
        {
            ct.ThrowIfCancellationRequested();

            var now = DateTime.Now;
            var o = new Order
            {
                CustomerId = customerId,
                OrderStatus = OrderStatus.WAITING_PAYMENT,
                CreatedAt = now,
                Deadline = now.AddDays(1),
                Amount = myCart.Sum(cc => cc.Product!.Price * cc.Quantity),
            };
            await this.ctx.Orders.AddAsync(o, ct);

            //NOTE: IDK IF IT IS THE RIGHT PLACE TO CREATE ORDER TRANSACTION
            // var ot = new OrderTransaction
            // {
            //     OrderId = o.Id,
            //     Order = o,
            //     PaymentMethod = PaymentMethod.CREDIT_CARD,
            //     PaymentStatus = PaymentStatus.PENDING
            // };
            // await this.ctx.OrderTransactions.AddAsync(ot, ct);

            //NOTE: INSTEAD PERFORM EXCLUSIVE LOCK ONE BY ONE
            //RETRIEVE ALL RELATED PRODUCTS AND PROCESS IN APPLICATION LAYER (PROGRAM)
            // foreach (var item in myCart)
            // {
            //     var p = await this.ctx.Products
            //         .FromSqlInterpolated(
            //                 $"SELECT * FROM Products WITH (UPDLOCK, ROWLOCK) WHERE Id = {item.ProductId}"
            //                 )
            //         .SingleAsync(ct);
            //
            //     if (p.Stock < item.Quantity)
            //     {
            //         throw new Exception($"Insufficient Product Stock");
            //     }
            //
            //     p.Stock -= item.Quantity;
            //
            //     o.OrderItems.Add(new OrderItem
            //             {
            //             OrderId = o.Id,
            //             ProductId = item.ProductId,
            //             Quantity = item.Quantity,
            //             });
            // }

            var productIds = myCart
                .Select(cc => cc.ProductId)
                .ToList();
            var products = await this.ctx.Products
                .FromSqlInterpolated(
                    $"SELECT * FROM Products WITH (UPDLOCK, ROWLOCK) WHERE Id IN ({string.Join(",", productIds)})"
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
                $"DELETE FROM CustomerCarts WHERE CustomerId = {customerId}", ct);
            await this.ctx.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

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
}
