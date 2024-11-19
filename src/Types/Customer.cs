using System.ComponentModel.DataAnnotations;
using Riok.Mapperly.Abstractions;

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
    CUSTOMER = 0,
    ADMIN = 1
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
public record CustomerOverviewDTO(int id, string name, string email, UserRoles role);
public record LoginDTO(string email, string password);
public record CreateCustomerDTO(string name, string email, string password);
public record CreateCustomerAndCustomerAddressDTO(CreateCustomerDTO custData, UpsertCustomerAddressDTO? addrData);
public record EditCustomerDTO(string name, string email, UserRoles role);
public record EditCustomerAndCustomerAddressDTO(EditCustomerDTO custData, CustomerAddress? addrData);
public record CustomerCartDTO(int productId, int quantity);

[Mapper]
public static partial class CustomerMapper
{
    public static partial CustomerOverviewDTO ToCustomerOverviewDTO(this Customer c);
    public static partial IQueryable<CustomerOverviewDTO> ToCustomersOverviewDTO(this IQueryable<Customer> c);
}
