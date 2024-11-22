using ECommerce.Service.Interface;
using ECommerce.Store;
using ECommerce.Types;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Service;

public class CustomerCartService : ICustomerCartService
{
    private readonly ApplicationDbContext ctx;
    private readonly ILogger<CustomerCartService> logger;

    public CustomerCartService(
        ApplicationDbContext ctx,
        ILogger<CustomerCartService> logger)
    {
        this.ctx = ctx;
        this.logger = logger;
    }

    public async Task<bool> AddItemToCart(
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
            var result = await this.ctx.SaveChangesAsync(ct) > 0;

            return result;
        }
        catch (System.Exception err)
        {
            this.logger.LogError($"Error in {err.Source} - {err.Message}");
            throw;
        }
    }

    public async Task<CustomerCart> EditItemQuantity(
        CancellationToken ct,
        int quantity,
        CustomerCart cc)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            cc.Quantity = quantity;

            this.ctx.CustomerCarts.Update(cc);
            await this.ctx.SaveChangesAsync(ct);

            return cc;
        }
        catch (System.Exception err)
        {
            this.logger.LogError($"Error in {err.Source} - {err.Message}");
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
            this.logger.LogError($"Error in {err.Source} - {err.Message}");
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
            this.logger.LogError($"Error in {err.Source} - {err.Message}");
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
            this.logger.LogError($"Error in {err.Source} - {err.Message}");
            throw;
        }
    }

    public IQueryable<CustomerCart> FindItemsInMyCart(
        CancellationToken ct,
        int customerId)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var query = this.ctx
                .CustomerCarts
                .AsNoTracking()
                .Include(cc => cc.Product)
                    .ThenInclude(p => p!.ProductCategory)
                .Where(cc => cc.CustomerId.Equals(customerId));

            return query;
        }
        catch (System.Exception err)
        {
            this.logger.LogError($"Error in {err.Source} - {err.Message}");
            throw;
        }
    }

    public async Task<bool> RemoveItemFromCart(
        CancellationToken ct,
        CustomerCart item)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            int customerId = item.CustomerId;

            this.ctx.CustomerCarts.Remove(item);
            var result = await this.ctx.SaveChangesAsync() > 0;

            return result;
        }
        catch (System.Exception err)
        {
            this.logger.LogError($"Error in {err.Source} - {err.Message}");
            throw;
        }
    }
}
