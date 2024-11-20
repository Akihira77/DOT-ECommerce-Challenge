using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Types;

public class Product
{
    [Key]
    public int Id { get; set; }
    public required string Name { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public int? ProductCategoryId { get; set; }
    public ProductCategory? ProductCategory { get; set; }

    public ICollection<CustomerCart> CustomerCarts { get; set; } = new List<CustomerCart>();
}

public class ProductDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Description { get; set; } = string.Empty;
    public ProductCategoryDTO? ProductCategory { get; set; }
}
public record CreateProductDTO(
    string name,
    decimal price,
    int stock,
    string description,
    int productCategoryId);
public record EditProductDTO(
    string name,
    decimal price,
    int stock,
    string description);
public record FindProductsQueryDTO(
    string name,
    decimal minPrice,
    decimal maxPrice,
    bool includeProductCategory);
