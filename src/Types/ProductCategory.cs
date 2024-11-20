using System.ComponentModel.DataAnnotations;

namespace ECommerce.Types;

public class ProductCategory
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public uint ProductCount { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public record ProductCategoryDTO(int id, string name, string description);
public record UpsertProductCategoryDTO(string name, string description);
public record FindProductCategoriesQueryDTO(bool products);
