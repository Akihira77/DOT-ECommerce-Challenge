using System.ComponentModel.DataAnnotations;

namespace ECommerce.Types;

public class CustomerAddress
{
    [Key]
    public int Id { get; set; }
    public required string Country { get; set; }
    public required string State { get; set; }
    public required string FullAddress { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
}
