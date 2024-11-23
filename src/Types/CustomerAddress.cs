using System.ComponentModel.DataAnnotations;
using FluentValidation;

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

public record UpsertCustomerAddressDTO(string country, string state, string fullAddress);

public class UpsertCustomerAddressValidator : AbstractValidator<UpsertCustomerAddressDTO>
{
    public UpsertCustomerAddressValidator()
    {
        RuleFor(x => x.country)
            .NotNull().WithMessage("Country address cannot be null")
            .NotEmpty().WithMessage("Country address cannot be empty");

        RuleFor(x => x.state)
            .NotNull().WithMessage("State address cannot be null")
            .NotEmpty().WithMessage("State address cannot be empty");

        RuleFor(x => x.fullAddress)
            .NotNull().WithMessage("Full Address cannot be null")
            .NotEmpty().WithMessage("Full Address cannot be empty");
    }
}

public class CustomerAddressValidator : AbstractValidator<CustomerAddress>
{
    public CustomerAddressValidator()
    {
        RuleFor(x => x.Country)
            .NotNull().WithMessage("Country address cannot be null")
            .NotEmpty().WithMessage("Country address cannot be empty");

        RuleFor(x => x.State)
            .NotNull().WithMessage("State address cannot be null")
            .NotEmpty().WithMessage("State address cannot be empty");

        RuleFor(x => x.FullAddress)
            .NotNull().WithMessage("Full Address cannot be null")
            .NotEmpty().WithMessage("Full Address cannot be empty");
    }
}

