using System.ComponentModel.DataAnnotations;

namespace ECommerce.Types;

public class Order
{
    [Key]
    public int Id { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public DateTime CreatedAt { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public OrderTransaction? Transaction { get; set; }
    public CustomerOrderHistory? CustomerOrderHistory { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

public enum OrderStatus
{
    WAITING,
    PROCESS,
    SHIP,
    COMPLETE
}

public class OrderItem
{
    [Key]
    public int Int { get; set; }
    public int Quantity { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }
}

public record CreateOrderDTO(IEnumerable<CustomerCartDTO> myCart);
public record OrderJob(int orderId, int customerId, IEnumerable<CustomerCart> myCart);
