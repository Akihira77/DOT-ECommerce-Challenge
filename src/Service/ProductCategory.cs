using ECommerce.Service.Interface;
using ECommerce.Store;
using ECommerce.Types;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Service;

public class ProductCategoryService : IProductCategoryService
{
    private readonly ApplicationDbContext ctx;

    public ProductCategoryService(ApplicationDbContext ctx)
    {
        this.ctx = ctx;
    }

    public async Task<ProductCategory> CreateProductCategory(
        CancellationToken ct,
        UpsertProductCategoryDTO data)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var pc = new ProductCategory
            {
                Name = data.name,
                Description = data.description,
                ProductCount = 0,
            };

            this.ctx.ProductCategories.Add(pc);
            await this.ctx.SaveChangesAsync(ct);

            return pc;
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            throw;
        }
    }

    public async Task<bool> DeleteProductCategory(
        CancellationToken ct,
        ProductCategory pc)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            this.ctx.ProductCategories.Remove(pc);
            return await this.ctx.SaveChangesAsync(ct) > 0;
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            throw;
        }
    }

    public async Task<ProductCategory> EditProductCategory(
        CancellationToken ct,
        UpsertProductCategoryDTO data,
        ProductCategory pc)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            pc.Name = data.name;
            pc.Description = data.description;

            this.ctx.ProductCategories.Update(pc);
            await this.ctx.SaveChangesAsync(ct);

            return pc;
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            throw;
        }
    }

    public async Task<IEnumerable<ProductCategory>> FindProductCategories(
        CancellationToken ct,
        bool track,
        bool includeProducts)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var query = this.ctx.ProductCategories.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            if (includeProducts)
            {
                query = query.Include(pc => pc.Products);
            }

            return await query.ToListAsync(ct);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return [];
        }
    }

    public async Task<ProductCategory?> FindProductCategoryById(
        CancellationToken ct,
        int id,
        bool track,
        bool includeProducts)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var query = this.ctx.ProductCategories.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            if (includeProducts)
            {
                query = query.Include(pc => pc.Products);
            }

            return await query.FirstOrDefaultAsync(pc => pc.Id.Equals(id), ct);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            throw;
        }
    }
}
