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

    public ICollection<CustomerAddress> CustomerAddresses { get; set; } = new List<CustomerAddress>();
    public ICollection<CustomerOrderHistory> CustomerOrderHistories { get; set; } = new List<CustomerOrderHistory>();
    public ICollection<CustomerCart> CustomerCarts { get; set; } = new List<CustomerCart>();
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

    public int Quantity { get; set; } = 1;
}

public enum ChangeItemQuantity
{
    INCREASE_OR_DECREASE,
    CHANGE
}

// DTO excluding password
public record LoginDTO(string email, string password);
public record CreateCustomerDTO(string name, string email, string password);
public record CreateCustomerAndCustomerAddressDTO(CreateCustomerDTO custData, UpsertCustomerAddressDTO? addrData);
public record EditCustomerDTO(string name, string email, UserRoles role);
public record EditCustomerAndCustomerAddressDTO(EditCustomerDTO custData, CustomerAddress? addrData);
public record CustomerCartDTO(int productId, int quantity);
