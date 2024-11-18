using ECommerce.Store;
using ECommerce.Types;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Service;

public class CustomerCartService : ICustomerCartService
{
    private readonly ApplicationDbContext ctx;
    public CustomerCartService(ApplicationDbContext ctx)
    {
        this.ctx = ctx;
    }

    public async Task<IEnumerable<CustomerCart>> AddItemToCart(
        CancellationToken ct,
        int customerId,
        CustomerCartDTO item)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var cc = new CustomerCart
            {
                CustomerId = customerId,
                ProductId = item.productId,
                Quantity = item.quantity,
            };
            this.ctx.CustomerCarts.Add(cc);
            await this.ctx.SaveChangesAsync(ct);

            return await this.FindItemsInMyCart(ct, customerId);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            throw;
        }
    }

    public async Task<CustomerCart> EditItemQuantity(
        CancellationToken ct,
        int quantity,
        ChangeItemQuantity ciq,
        CustomerCart cc)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            if (ciq.Equals(ChangeItemQuantity.INCREASE_OR_DECREASE))
            {
                cc.Quantity += quantity;
            }
            else
            {
                cc.Quantity = quantity;
            }

            this.ctx.CustomerCarts.Update(cc);
            await this.ctx.SaveChangesAsync(ct);

            return cc;
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            throw;
        }
    }

    public async Task<CustomerCart?> FindCartItemById(
        CancellationToken ct,
        int cartItemId,
        bool track)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var query = this.ctx.CustomerCarts.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(cc => cc.Id.Equals(cartItemId), ct);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            throw;
        }
    }

    public async Task<CustomerCart?> FindCartItemInMyCartByProductId(
        CancellationToken ct,
        int customerId,
        int productId,
        bool track,
        bool includeProduct)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var query = this.ctx.CustomerCarts.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            if (includeProduct)
            {
                query = query.Include(cc => cc.Product);
            }

            return await query.FirstOrDefaultAsync(
                    cc => cc.CustomerId.Equals(customerId) &&
                    cc.ProductId.Equals(productId),
                    ct);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            throw;
        }
    }

    public async Task<CustomerCart?> FindCartItemInMyCartById(
        CancellationToken ct,
        int customerId,
        int cartItemId,
        bool track,
        bool includeProduct)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var query = this.ctx.CustomerCarts.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            if (includeProduct)
            {
                query = query.Include(cc => cc.Product);
            }

            return await query.FirstOrDefaultAsync(
                    cc => cc.CustomerId.Equals(customerId) &&
                    cc.Id.Equals(cartItemId),
                    ct);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            throw;
        }
    }

    public async Task<IEnumerable<CustomerCart>> FindItemsInMyCart(
        CancellationToken ct,
        int customerId)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var myCart = await this.ctx
                .CustomerCarts
                .AsNoTracking()
                .Include(cc => cc.Product)
                .Where(cc => cc.CustomerId.Equals(customerId))
                .ToListAsync(ct);

            return myCart;
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            throw;
        }
    }

    public async Task<IEnumerable<CustomerCart>> RemoveItemFromCart(
        CancellationToken ct,
        CustomerCart item)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            int customerId = item.CustomerId;

            this.ctx.CustomerCarts.Remove(item);
            await this.ctx.SaveChangesAsync();

            return await this.FindItemsInMyCart(ct, customerId);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            throw;
        }
    }
}
