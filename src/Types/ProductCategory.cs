using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FluentValidation;

namespace ECommerce.Types;

public class ProductCategory
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    [Column(TypeName = "decimal(5,2)")]
    public decimal DiscountPercentage { get; set; }
    public uint ProductCount { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public record ProductCategoryDTO(int id, string name, string description, decimal discountPercentage);
public record UpsertProductCategoryDTO(string name, string description, decimal discountPercentage);
public record FindProductCategoriesQueryDTO(bool products);

public class UpsertProductCategoryValidator : AbstractValidator<UpsertProductCategoryDTO>
{
    public UpsertProductCategoryValidator()
    {
        RuleFor(x => x.name)
            .NotNull().WithMessage("Product category name cannot be null")
            .NotEmpty().WithMessage("Product category name cannot be empty");

        RuleFor(x => x.description)
            .NotNull().WithMessage("Product category description cannot be null")
            .NotEmpty().WithMessage("Product category description cannot be empty");

        RuleFor(x => x.discountPercentage)
            .NotNull().WithMessage("Product category discount cannot be null")
            .NotEmpty().WithMessage("Product category discount cannot be empty")
            .LessThanOrEqualTo(100.00m).WithMessage("Product category discount cannot exceed 100.00%")
            .GreaterThan(0.00m).WithMessage("Product category discount cannot lower than 0.00%");
    }
}
