using ECommerce.Types;

namespace ECommerce.Service;

public interface IProductService
{
    Task<IEnumerable<Product>> FindProducts(
        CancellationToken ct,
        bool track,
        bool includeProductCategory);
    Task<Product?> FindProductById(
        CancellationToken ct,
        int id,
        bool track,
        bool includeProductCategory);
    Task<Product> CreateProduct(
        CancellationToken ct,
        CreateProductDTO data,
        ProductCategory pc);
    Task<Product> EditProduct(
        CancellationToken ct,
        EditProductDTO data,
        Product p);
    Task<bool> DeleteProduct(
        CancellationToken ct,
        Product p,
        ProductCategory pc);
}

