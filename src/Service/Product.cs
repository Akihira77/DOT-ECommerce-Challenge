using ECommerce.Store;
using ECommerce.Types;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Service;

public interface IProductService
{
    Task<IEnumerable<Product>> FindProducts(bool track, bool includeProductCategory);
    Task<Product?> FindProductById(int id, bool track, bool includeProductCategory);
    Task<Product> CreateProduct(CreateProductDTO data, ProductCategory pc);
    Task<Product> EditProduct(EditProductDTO data, Product p);
    Task<bool> DeleteProduct(Product p, ProductCategory pc);
}

public class ProductService : IProductService
{
    private readonly ApplicationDbContext ctx;

    public ProductService(ApplicationDbContext ctx)
    {
        this.ctx = ctx;
    }

    public async Task<Product> CreateProduct(CreateProductDTO data, ProductCategory pc)
    {
        using var tx = this.ctx.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
        try
        {
            var p = new Product
            {
                Name = data.name,
                Description = data.description,
                Price = data.price,
                Stock = data.stock,
                ProductCategoryId = data.productCategoryId,
                CreatedAt = DateTime.Now
            };

            this.ctx.Products.Add(p);
            await this.ctx.SaveChangesAsync();

            pc.ProductCount += 1;
            this.ctx.ProductCategories.Update(pc);
            await this.ctx.SaveChangesAsync();

            tx.Commit();
            return p;
        }
        catch (System.Exception err)
        {
            tx.Rollback();
            Console.WriteLine($"There are errors {err}");
            throw;
        }
    }

    public async Task<bool> DeleteProduct(Product p, ProductCategory pc)
    {
        using var tx = this.ctx.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
        try
        {
            this.ctx.Products.Remove(p);
            await this.ctx.SaveChangesAsync();

            pc.ProductCount -= 1;
            this.ctx.ProductCategories.Update(pc);
            var result = await this.ctx.SaveChangesAsync();

            tx.Commit();
            return result > 0;
        }
        catch (System.Exception err)
        {
            tx.Rollback();
            Console.WriteLine($"There are errors {err}");
            return false;
        }
    }

    public async Task<Product> EditProduct(EditProductDTO data, Product p)
    {
        try
        {
            p.Name = data.name;
            p.Description = data.description;
            p.Stock = data.stock;
            p.Price = data.price;

            this.ctx.Products.Update(p);
            await this.ctx.SaveChangesAsync();

            return p;
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            throw;
        }
    }

    public async Task<Product?> FindProductById(int id, bool track, bool includeProductCategory)
    {
        try
        {
            var query = this.ctx.Products.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            if (includeProductCategory)
            {
                query = query.Include(p => p.ProductCategory);
            }

            return await query.FirstOrDefaultAsync(p => p.Id.Equals(id));
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return null;
        }
    }

    public async Task<IEnumerable<Product>> FindProducts(bool track, bool includeProductCategory)
    {
        try
        {
            var query = this.ctx.Products.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            if (includeProductCategory)
            {
                query = query.Include(p => p.ProductCategory);
            }

            return await query.ToListAsync();
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return [];
        }
    }
}
