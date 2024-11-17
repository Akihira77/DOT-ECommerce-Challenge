using ECommerce.Store;
using ECommerce.Types;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Service;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext ctx;

    public ProductService(ApplicationDbContext ctx)
    {
        this.ctx = ctx;
    }

    public async Task<Product> CreateProduct(
        CancellationToken ct,
        CreateProductDTO data,
        ProductCategory pc)
    {
        using var tx = this.ctx.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
        try
        {
            ct.ThrowIfCancellationRequested();

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

            pc.ProductCount += 1;
            this.ctx.ProductCategories.Update(pc);
            await this.ctx.SaveChangesAsync(ct);

            await tx.CommitAsync(ct);
            return p;
        }
        catch (System.Exception err)
        {
            await tx.RollbackAsync(ct);
            Console.WriteLine($"There are errors {err}");
            throw;
        }
    }

    public async Task<bool> DeleteProduct(
        CancellationToken ct,
        Product p,
        ProductCategory pc)
    {
        using var tx = this.ctx.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
        try
        {
            ct.ThrowIfCancellationRequested();

            this.ctx.Products.Remove(p);

            pc.ProductCount -= 1;
            this.ctx.ProductCategories.Update(pc);
            var result = await this.ctx.SaveChangesAsync(ct);

            await tx.CommitAsync(ct);
            return result > 0;
        }
        catch (System.Exception err)
        {
            await tx.RollbackAsync(ct);
            Console.WriteLine($"There are errors {err}");
            throw;
        }
    }

    public async Task<Product> EditProduct(
        CancellationToken ct,
        EditProductDTO data,
        Product p)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            p.Name = data.name;
            p.Description = data.description;
            p.Stock = data.stock;
            p.Price = data.price;

            this.ctx.Products.Update(p);
            await this.ctx.SaveChangesAsync(ct);

            return p;
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            throw;
        }
    }

    public async Task<Product?> FindProductById(
        CancellationToken ct,
        int id,
        bool track,
        bool includeProductCategory)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var query = this.ctx.Products.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            if (includeProductCategory)
            {
                query = query.Include(p => p.ProductCategory);
            }

            return await query.FirstOrDefaultAsync(p => p.Id.Equals(id), ct);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            throw;
        }
    }

    public async Task<IEnumerable<Product>> FindProducts(
        CancellationToken ct,
        bool track,
        bool includeProductCategory)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var query = this.ctx.Products.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            if (includeProductCategory)
            {
                query = query.Include(p => p.ProductCategory);
            }

            return await query.ToListAsync(ct);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return [];
        }
    }
}
