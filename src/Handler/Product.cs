using ECommerce.Service.Interface;
using ECommerce.Types;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Handler;

public static class ProductHandler
{
    public static async Task<IResult> FindProducts(
        HttpContext httpCtx,
        IValidator<FindProductsQueryDTO> validator,
        [FromServices] IProductService productSvc,
        [FromQuery] string name = "",
        [FromQuery] decimal minPrice = 0,
        [FromQuery] decimal maxPrice = Int32.MaxValue,
        [FromQuery] bool includeProductCategory = false)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var query = new FindProductsQueryDTO(name, minPrice, maxPrice, includeProductCategory);
            var validationResult = await validator.ValidateAsync(query, cts.Token);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var ps = await productSvc.FindProducts(cts.Token, false, query);

            return Results.Ok(ps);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }

    public static async Task<IResult> FindProductById(
        HttpContext httpCtx,
        [FromServices] IProductService productSvc,
        [FromRoute] int id,
        [FromQuery] bool includeProductCategory = false)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var p = await productSvc.FindProductById(cts.Token, id, false, includeProductCategory);
            if (p is null)
            {
                return new NotFoundError("Product data is not found").ToResult();
            }

            return Results.Ok(p);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }

    public static async Task<IResult> CreateProduct(
        HttpContext httpCtx,
        IValidator<CreateProductDTO> validator,
        [FromServices] IProductService productSvc,
        [FromServices] IProductCategoryService productCategorySvc,
        [FromBody] CreateProductDTO body)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var validationResult = await validator.ValidateAsync(body, cts.Token);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var pc = await productCategorySvc.FindProductCategoryById(cts.Token, body.productCategoryId, false, false);
            if (pc is null)
            {
                return new BadRequestError("Invalid Product Category").ToResult();
            }

            var p = await productSvc.CreateProduct(cts.Token, body, pc);
            if (p is null)
            {
                return new BadRequestError("Failed Creating Product").ToResult();
            }

            return Results.Created(httpCtx.Request.Path, p);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }

    public static async Task<IResult> EditProduct(
        HttpContext httpCtx,
        IValidator<EditProductDTO> validator,
        [FromServices] IProductService productSvc,
        [FromRoute] int productId,
        [FromBody] EditProductDTO body)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var validationResult = await validator.ValidateAsync(body, cts.Token);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var p = await productSvc.FindProductById(cts.Token, productId, false, true);
            if (p is null)
            {
                return new NotFoundError("Product is not found").ToResult();
            }

            p = await productSvc.EditProduct(cts.Token, body, p);

            return Results.Ok(p);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }

    public static async Task<IResult> DeleteProduct(
        HttpContext httpCtx,
        [FromServices] IProductService productSvc,
        [FromRoute] int productId)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var p = await productSvc.FindProductById(cts.Token, productId, false, true);
            if (p is null)
            {
                return new NotFoundError("Product is not found").ToResult();
            }

            var result = await productSvc.DeleteProduct(cts.Token, p, p.ProductCategory!);
            if (!result)
            {
                return new BadRequestError("Deleting Product Failed").ToResult();
            }

            return Results.Ok("Deleting Product Success");
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }

}
