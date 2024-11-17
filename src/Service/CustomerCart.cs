using ECommerce.Store;
using ECommerce.Types;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Service;

public interface ICustomerCartService
{
    Task<IEnumerable<CustomerCart>> FindItemsInMyCart(int customerId);
    Task<CustomerCart?> FindCartItemById(int cartItemId, bool track);
    Task<CustomerCart?> FindCartItemInMyCartById(int customerId, int cartItemId, bool track, bool includeProduct);
    Task<IEnumerable<CustomerCart>> AddItemToCart(int customerId, CustomerCartDTO item);
    Task<CustomerCart> EditItemQuantity(int quantity, ChangeItemQuantity ciq, CustomerCart cc);
    Task<IEnumerable<CustomerCart>> RemoveItemFromCart(CustomerCart item);
}

public class CustomerCartService : ICustomerCartService
{
    private readonly ApplicationDbContext ctx;
    public CustomerCartService(ApplicationDbContext ctx)
    {
        this.ctx = ctx;
    }

    public async Task<IEnumerable<CustomerCart>> AddItemToCart(int customerId, CustomerCartDTO item)
    {
        try
        {
            var cc = new CustomerCart
            {
                CustomerId = customerId,
                ProductId = item.productId,
                Quantity = item.quantity,
            };
            this.ctx.CustomerCarts.Add(cc);
            await this.ctx.SaveChangesAsync();

            return await this.FindItemsInMyCart(customerId);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            throw;
        }
    }

    public async Task<CustomerCart> EditItemQuantity(int quantity, ChangeItemQuantity ciq, CustomerCart cc)
    {
        try
        {
            if (ciq.Equals(ChangeItemQuantity.INCREASE_OR_DECREASE))
            {
                cc.Quantity += quantity;
            }
            else
            {
                cc.Quantity = quantity;
            }

            this.ctx.CustomerCarts.Update(cc);
            await this.ctx.SaveChangesAsync();

            return cc;
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            throw;
        }
    }

    public async Task<CustomerCart?> FindCartItemById(int cartItemId, bool track)
    {
        try
        {
            var query = this.ctx.CustomerCarts.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(cc => cc.Id.Equals(cartItemId));
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            throw;
        }
    }

    public async Task<CustomerCart?> FindCartItemInMyCartById(int customerId, int cartItemId, bool track, bool includeProduct)
    {
        try
        {
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
                    cc.Id.Equals(cartItemId));
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            throw;
        }
    }

    public async Task<IEnumerable<CustomerCart>> FindItemsInMyCart(int customerId)
    {
        try
        {
            var myCart = await this.ctx
                .CustomerCarts
                .AsNoTracking()
                .Where(cc => cc.CustomerId.Equals(customerId))
                .ToListAsync();

            return myCart;
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            throw;
        }
    }

    public async Task<IEnumerable<CustomerCart>> RemoveItemFromCart(CustomerCart item)
    {
        try
        {
            int customerId = item.CustomerId;

            this.ctx.CustomerCarts.Remove(item);
            await this.ctx.SaveChangesAsync();

            return await this.FindItemsInMyCart(customerId);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            throw;
        }
    }
}
