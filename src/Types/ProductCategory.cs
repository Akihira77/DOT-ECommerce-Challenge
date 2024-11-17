using System.ComponentModel.DataAnnotations;

namespace ECommerce.Types;

public class ProductCategory
{
    [Key]
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public uint ProductCount { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public record UpsertProductCategoryDTO(string name, string description);
public record FindProductCategoriesQueryDTO(bool products);
