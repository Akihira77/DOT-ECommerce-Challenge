using System.ComponentModel.DataAnnotations;

namespace ECommerce.Types;

public class ProductCategory
{
    [Key]
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }

    public ICollection<Product> Products { get; set; }
}
