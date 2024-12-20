using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Types;

public class Order
{
    [Key]
    public int Id { get; set; }
    public OrderStatus OrderStatus { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime Deadline { get; set; }
    public int Version { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public OrderTransaction? OrderTransaction { get; set; }
    public CustomerOrderHistory? CustomerOrderHistory { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

public enum OrderStatus
{
    WAITING_PAYMENT = 0,
    EXPIRED = 1,
    PROCESS = 2,
    SHIP = 3,
    COMPLETE = 4
}

public class OrderItem
{
    [Key]
    public int Int { get; set; }
    public int Quantity { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal Amount { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }
}

public record CreateOrderDTO(IEnumerable<CustomerCartDTO> myCart);
public record FindOrderQueryDTO(string orderStatus);
public record UpdateOrderDTO(string orderStatus);
