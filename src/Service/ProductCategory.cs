using ECommerce.Store;
using ECommerce.Types;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Service;

public interface IProductCategoryService
{
    Task<IEnumerable<ProductCategory>> FindProductCategories(bool track, bool includeProducts);
    Task<ProductCategory?> FindProductCategoryById(int id, bool track, bool includeProducts);
    Task<ProductCategory?> CreateProductCategory(UpsertProductCategoryDTO data);
    Task<ProductCategory?> EditProductCategory(UpsertProductCategoryDTO data, ProductCategory pc);
    Task<bool> DeleteProductCategory(ProductCategory pc);
}

public class ProductCategoryService : IProductCategoryService
{
    private readonly ApplicationDbContext ctx;

    public ProductCategoryService(ApplicationDbContext ctx)
    {
        this.ctx = ctx;
    }

    public async Task<ProductCategory?> CreateProductCategory(UpsertProductCategoryDTO data)
    {
        try
        {
            var pc = new ProductCategory
            {
                Name = data.name,
                Description = data.description,
                ProductCount = 0,
            };

            await this.ctx.ProductCategories.AddAsync(pc);
            await this.ctx.SaveChangesAsync();

            return pc;
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return null;
        }
    }

    public async Task<bool> DeleteProductCategory(ProductCategory pc)
    {
        try
        {
            this.ctx.ProductCategories.Remove(pc);
            return await this.ctx.SaveChangesAsync() > 0;
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return false;
        }
    }

    public async Task<ProductCategory?> EditProductCategory(UpsertProductCategoryDTO data, ProductCategory pc)
    {
        try
        {
            pc.Name = data.name;
            pc.Description = data.description;

            this.ctx.ProductCategories.Update(pc);
            await this.ctx.SaveChangesAsync();

            return pc;
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return null;
        }
    }

    public async Task<IEnumerable<ProductCategory>> FindProductCategories(bool track, bool includeProducts)
    {
        try
        {
            var query = this.ctx.ProductCategories.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            if (includeProducts)
            {
                query = query.Include(pc => pc.Products);
            }

            return await query.ToListAsync();
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return [];
        }
    }

    public async Task<ProductCategory?> FindProductCategoryById(int id, bool track, bool includeProducts)
    {
        try
        {
            var query = this.ctx.ProductCategories.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            if (includeProducts)
            {
                query = query.Include(pc => pc.Products);
            }


            return await query.FirstOrDefaultAsync(pc => pc.Id.Equals(id));
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return null;
        }
    }
}
