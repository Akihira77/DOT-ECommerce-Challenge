using ECommerce.Service.Interface;
using ECommerce.Types;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Handler;

public static class ProductCategoryHandler
{
    public static async Task<IResult> FindProductCategories(
        HttpContext httpCtx,
        [FromServices] IProductCategoryService productCategorySvc,
        [FromQuery] bool includeProducts = false)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var productCategories = await productCategorySvc.FindProductCategories(cts.Token, false, includeProducts);

            return Results.Ok(productCategories);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }

    public static async Task<IResult> FindProductCategoryById(
        HttpContext httpCtx,
        [FromServices] IProductCategoryService productCategorySvc,
        [FromRoute] int id,
        [FromQuery] bool includeProducts = false)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var pc = await productCategorySvc.FindProductCategoryById(cts.Token, id, false, includeProducts);
            if (pc is null)
            {
                return new NotFoundError("Product Category is not found").ToResult();
            }

            return Results.Ok(pc);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }

    public static async Task<IResult> CreateProductCategory(
        HttpContext httpCtx,
        [FromServices] IValidator<UpsertProductCategoryDTO> validator,
        [FromServices] IProductCategoryService productCategorySvc,
        [FromBody] UpsertProductCategoryDTO body)
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

            var pc = await productCategorySvc.CreateProductCategory(cts.Token, body);

            return Results.Created(httpCtx.Request.Path, pc);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }

    public static async Task<IResult> EditProductCategory(
        HttpContext httpCtx,
        [FromServices] IValidator<UpsertProductCategoryDTO> validator,
        [FromServices] IProductCategoryService productCategorySvc,
        [FromRoute] int categoryId,
        [FromBody] UpsertProductCategoryDTO body)
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

            var pc = await productCategorySvc.FindProductCategoryById(cts.Token, categoryId, false, false);
            if (pc is null)
            {
                return new NotFoundError("Product Category is not found").ToResult();
            }

            pc = await productCategorySvc.EditProductCategory(cts.Token, body, pc);

            return Results.Ok(pc);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }

    public static async Task<IResult> DeleteProductCategory(
        HttpContext httpCtx,
        [FromServices] IProductCategoryService productCategorySvc,
        [FromRoute] int categoryId)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var pc = await productCategorySvc.FindProductCategoryById(cts.Token, categoryId, false, false);
            if (pc is null)
            {
                return new NotFoundError("Product Category is not found").ToResult();
            }

            var result = await productCategorySvc.DeleteProductCategory(cts.Token, pc);
            if (!result)
            {
                return new BadRequestError("Deleting Product Category failed").ToResult();
            }

            return Results.Ok("Deleting Product Category success");
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }
}
