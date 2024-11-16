using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Types;

public class Product
{
    [Key]
    public int Id { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal Price { get; set; }
    public uint Stock { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public int? CategoryId { get; set; }
    public ProductCategory? Category { get; set; }

    public ICollection<CustomerCart> CustomerCarts { get; set; }
}
