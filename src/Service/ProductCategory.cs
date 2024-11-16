using ECommerce.Store;
using ECommerce.Types;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Service;

public interface IProductCategoryService
{
    Task<IEnumerable<ProductCategory>> FindProductCategories(bool track);
    Task<ProductCategory?> FindProductCategoryById(int id, bool track);
    Task<ProductCategory?> CreateProductCategory(UpsertProductCategoryDTO data);
    Task<ProductCategory?> EditProductCategory(UpsertProductCategoryDTO data, ProductCategory pc);
    Task<bool> DeleteProductCategory(int id);
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

    public Task<bool> DeleteProductCategory(int id)
    {
        throw new NotImplementedException();
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

    public async Task<IEnumerable<ProductCategory>> FindProductCategories(bool track)
    {
        try
        {
            var query = this.ctx.ProductCategories.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            return await query.ToListAsync();
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return [];
        }
    }

    public async Task<ProductCategory?> FindProductCategoryById(int id, bool track)
    {
        try
        {
            var query = this.ctx.ProductCategories.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
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
