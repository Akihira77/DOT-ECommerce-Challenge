using System.ComponentModel.DataAnnotations;
using FluentValidation;
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

public record CustomerOverviewDTO(int id, string name, string email, UserRoles role);
public record LoginDTO(string email, string password);
public record CreateCustomerDTO(string name, string email, string password);
public record CreateCustomerAndCustomerAddressDTO(CreateCustomerDTO custData, UpsertCustomerAddressDTO? addrData);
public record EditCustomerDTO(string name, string email, UserRoles role);
public record EditCustomerAndCustomerAddressDTO(EditCustomerDTO custData, CustomerAddress? addrData);
public record CustomerCartDTO(int productId, int quantity);
public record CustomerCartOverviewDTO(int id, ProductDTO product, int quantity);
public record EditCustomerCartDTO(int quantity);

[Mapper()]
public static partial class CustomerMapper
{
    public static partial CustomerOverviewDTO ToCustomerOverviewDTO(this Customer c);
    public static partial IQueryable<CustomerOverviewDTO> ToCustomersOverviewDTO(this IQueryable<Customer> c);
    public static partial CustomerCartOverviewDTO ToDTO(this CustomerCart cc);
    public static partial IQueryable<CustomerCartOverviewDTO> ToDTOS(this IQueryable<CustomerCart> cc);
}

public class LoginValidator : AbstractValidator<LoginDTO>
{
    public LoginValidator()
    {
        RuleFor(x => x.email)
            .NotNull().WithMessage("Email cannot be null")
            .NotEmpty().WithMessage("Email cannot be empty")
            .EmailAddress().WithMessage("Invalid Email");

        RuleFor(x => x.password)
            .NotNull().WithMessage("Password cannot be null")
            .NotEmpty().WithMessage("Password cannot be empty")
            .MinimumLength(8).WithMessage("Password minimum length is 8");
    }
}

public class CreateCustomerValidator : AbstractValidator<CreateCustomerDTO>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.email)
            .NotNull().WithMessage("Email cannot be null")
            .NotEmpty().WithMessage("Email cannot be empty")
            .EmailAddress().WithMessage("Invalid Email");

        RuleFor(x => x.password)
            .NotNull().WithMessage("Password cannot be null")
            .NotEmpty().WithMessage("Password cannot be empty")
            .MinimumLength(8).WithMessage("Password minimum length is 8");

        RuleFor(x => x.name)
            .NotNull().WithMessage("Name cannot be null")
            .NotEmpty().WithMessage("Name cannot be empty");
    }
}

public class EditCustomerValidator : AbstractValidator<EditCustomerDTO>
{
    public EditCustomerValidator()
    {
        RuleFor(x => x.email)
            .NotNull().WithMessage("Email cannot be null")
            .NotEmpty().WithMessage("Email cannot be empty")
            .EmailAddress().WithMessage("Invalid Email");

        RuleFor(x => x.name)
            .NotNull().WithMessage("Name cannot be null")
            .NotEmpty().WithMessage("Name cannot be empty");

        RuleFor(x => x.role)
            .IsInEnum().WithMessage("Role is invalid");
    }
}

public class CustomerCartValidator : AbstractValidator<CustomerCartDTO>
{
    public CustomerCartValidator()
    {
        RuleFor(x => x.productId)
            .NotNull().WithMessage("Product id cannot be null")
            .NotEmpty().WithMessage("Product id cannot be empty")
            .GreaterThan(0).WithMessage("Product id is invalid");

        RuleFor(x => x.quantity)
            .NotNull().WithMessage("Quantity cannot be null")
            .NotEmpty().WithMessage("Quantity cannot be empty")
            .GreaterThan(0).WithMessage("Quantity must greater than 0")
            .LessThanOrEqualTo(Int32.MaxValue).WithMessage("Quantity is too many");
    }
}

public class EditCustomerCartValidator : AbstractValidator<EditCustomerCartDTO>
{
    public EditCustomerCartValidator()
    {
        RuleFor(x => x.quantity)
            .NotNull().WithMessage("Quantity cannot be null")
            .NotEmpty().WithMessage("Quantity cannot be empty")
            .GreaterThan(0).WithMessage("Quantity must greater than 0")
            .LessThanOrEqualTo(Int32.MaxValue).WithMessage("Quantity is too many");
    }
}
