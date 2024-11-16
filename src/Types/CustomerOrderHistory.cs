using System.ComponentModel.DataAnnotations;

namespace ECommerce.Types;

public class CustomerOrderHistory
{
    [Key]
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int? OrderId { get; set; }
    public Order? Order { get; set; }
}
