using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FluentValidation;

namespace ECommerce.Types;

public class Product
{
    [Key]
    public int Id { get; set; }
    public required string Name { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Description { get; set; } = string.Empty;
    [Column(TypeName = "decimal(5,2)")]
    public decimal DiscountPercentage { get; set; }
    public DateTime CreatedAt { get; set; }

    public int? ProductCategoryId { get; set; }
    public ProductCategory? ProductCategory { get; set; }

    public ICollection<CustomerCart> CustomerCarts { get; set; } = new List<CustomerCart>();
}

public class ProductDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal DiscountPercentage { get; set; }
    public ProductCategoryDTO? ProductCategory { get; set; }
}
public record CreateProductDTO(
    string name,
    decimal price,
    int stock,
    string description,
    int productCategoryId,
    decimal discountPercentage);
public record EditProductDTO(
    string name,
    decimal price,
    int stock,
    string description,
    decimal discountPercentage);
public record FindProductsQueryDTO(
    string name,
    decimal minPrice,
    decimal maxPrice,
    bool includeProductCategory);

public class FindProductQueryValidator : AbstractValidator<FindProductsQueryDTO>
{
    public FindProductQueryValidator()
    {
        RuleFor(x => x.minPrice)
            .LessThanOrEqualTo(Int32.MaxValue).WithMessage("Product price is too large")
            .GreaterThanOrEqualTo(0).WithMessage("Product price is too low");

        RuleFor(x => x.maxPrice)
            .LessThanOrEqualTo(Int32.MaxValue).WithMessage("Product price is too large")
            .GreaterThanOrEqualTo(0).WithMessage("Product price is too low");

        RuleFor(x => x.minPrice)
            .Must((rootObj, minPrice, ctx) =>
            {
                return minPrice <= rootObj.maxPrice;
            }).WithMessage("Product min price must lower than or equal Product max price");
    }
}

public class CreateProductValidator : AbstractValidator<CreateProductDTO>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.name)
            .NotNull().WithMessage("Product name cannot be null")
            .NotEmpty().WithMessage("Product name cannot be empty");

        RuleFor(x => x.price)
            .LessThanOrEqualTo(Int32.MaxValue).WithMessage("Product price is too large")
            .GreaterThan(0).WithMessage("Product price is too low");

        RuleFor(x => x.stock)
            .LessThanOrEqualTo(Int32.MaxValue).WithMessage("Product stock is too large")
            .GreaterThan(0).WithMessage("Product stock is too low");

        RuleFor(x => x.description)
            .NotNull().WithMessage("Product description cannot be null")
            .NotEmpty().WithMessage("Product description cannot be empty");

        RuleFor(x => x.productCategoryId)
            .LessThanOrEqualTo(Int32.MaxValue).WithMessage("Product category is invalid")
            .GreaterThan(0).WithMessage("Product category is invalid");

        RuleFor(x => x.discountPercentage)
            .LessThanOrEqualTo(100.00m).WithMessage("Product discount cannot exceed 100.00%")
            .GreaterThan(0.00m).WithMessage("Product discount cannot lower than 0.00%");
    }
}

public class EditProductValidator : AbstractValidator<EditProductDTO>
{
    public EditProductValidator()
    {
        RuleFor(x => x.name)
            .NotNull().WithMessage("Product name cannot be null")
            .NotEmpty().WithMessage("Product name cannot be empty");

        RuleFor(x => x.price)
            .LessThanOrEqualTo(Int32.MaxValue).WithMessage("Product price is too much")
            .GreaterThan(0).WithMessage("Product price is too low");

        RuleFor(x => x.stock)
            .LessThanOrEqualTo(Int32.MaxValue).WithMessage("Product stock is too much")
            .GreaterThan(0).WithMessage("Product stock is too low");

        RuleFor(x => x.description)
            .NotNull().WithMessage("Product description cannot be null")
            .NotEmpty().WithMessage("Product description cannot be empty");

        RuleFor(x => x.discountPercentage)
            .NotNull().WithMessage("Product discount cannot be null")
            .NotEmpty().WithMessage("Product discount cannot be empty")
            .LessThanOrEqualTo(100.00m).WithMessage("Product discount cannot exceed 100.00%")
            .GreaterThan(0.00m).WithMessage("Product discount cannot lower than 0.00%");
    }
}

