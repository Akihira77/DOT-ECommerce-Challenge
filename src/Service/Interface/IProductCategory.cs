using ECommerce.Types;

namespace ECommerce.Service.Interface;

public interface IProductCategoryService
{
    Task<IEnumerable<ProductCategory>> FindProductCategories(
        CancellationToken ct,
        bool track,
        bool includeProducts);
    Task<ProductCategory?> FindProductCategoryById(
        CancellationToken ct,
        int id,
        bool track,
        bool includeProducts);
    Task<ProductCategory> CreateProductCategory(
        CancellationToken ct,
        UpsertProductCategoryDTO data);
    Task<ProductCategory> EditProductCategory(
        CancellationToken ct,
        UpsertProductCategoryDTO data,
        ProductCategory pc);
    Task<bool> DeleteProductCategory(
        CancellationToken ct,
        ProductCategory pc);
}
