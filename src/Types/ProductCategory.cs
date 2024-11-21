using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Types;

public class ProductCategory
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    [Column(TypeName = "decimal(5,2)")]
    public decimal DiscountPercentage { get; set; }
    public uint ProductCount { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public record ProductCategoryDTO(int id, string name, string description, decimal discountPercentage);
public record UpsertProductCategoryDTO(string name, string description, decimal discountPercentage);
public record FindProductCategoriesQueryDTO(bool products);
