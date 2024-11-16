using System.ComponentModel.DataAnnotations;

namespace ECommerce.Types;

public class Customer
{
    [Key]
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public UserRoles Role { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<CustomerAddress> CustomerAddresses { get; set; }
    public ICollection<CustomerOrderHistory> CustomerOrderHistories { get; set; }
    public ICollection<CustomerCart> CustomerCarts { get; set; }
}


public enum UserRoles
{
    CUSTOMER,
    ADMIN
}

public class CustomerCart
{
    [Key]
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public uint Quantity { get; set; } = 1;
}

// DTO excluding password
public record LoginDTO(string email, string password);
public record CreateCustomerDTO(string name, string email, string password);
public record EditCustomerDTO(string name, string email, UserRoles role);
