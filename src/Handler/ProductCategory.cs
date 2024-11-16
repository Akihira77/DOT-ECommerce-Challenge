using ECommerce.Service;
using ECommerce.Types;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Handler;

public static class ProductCategoryHandler
{
    public static async Task<Results<Ok<IEnumerable<ProductCategory>>, BadRequest<string>>> FindProductCategories(
        [FromServices] IProductCategoryService productCategorySvc,
        [FromQuery] bool includeProducts = false)
    {
        try
        {
            var productCategories = await productCategorySvc.FindProductCategories(false, includeProducts);

            return TypedResults.Ok(productCategories);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return TypedResults.BadRequest("Unexpected error happened.");
        }
    }

    public static async Task<Results<Ok<ProductCategory>, NotFound<string>, BadRequest<string>>> FindProductCategoryById(
        [FromServices] IProductCategoryService productCategorySvc,
        [FromRoute] int id,
        [FromQuery] bool includeProducts = false)
    {
        try
        {
            var productCategory = await productCategorySvc.FindProductCategoryById(id, false, includeProducts);
            if (productCategory is null)
            {
                return TypedResults.NotFound("Product Category did not found");
            }

            return TypedResults.Ok(productCategory);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return TypedResults.BadRequest("Unexpected error happened.");
        }
    }

    public static async Task<Results<Created<ProductCategory>, BadRequest<string>>> CreateProductCategory(
        HttpContext httpCtx,
        [FromServices] IProductCategoryService productCategorySvc,
        [FromBody] UpsertProductCategoryDTO data)
    {
        try
        {
            var productCategory = await productCategorySvc.CreateProductCategory(data);

            return TypedResults.Created(httpCtx.Request.Path, productCategory);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return TypedResults.BadRequest("Unexpected error happened.");
        }
    }

    public static async Task<Results<Ok<ProductCategory>, NotFound<string>, BadRequest<string>>> EditProductCategory(
        HttpContext httpCtx,
        [FromServices] IProductCategoryService productCategorySvc,
        [FromRoute] int categoryId,
        [FromBody] UpsertProductCategoryDTO data)
    {
        try
        {
            var productCategory = await productCategorySvc.FindProductCategoryById(categoryId, false, false);
            if (productCategory is null)
            {
                return TypedResults.NotFound("Product Category did not found");
            }

            productCategory = await productCategorySvc.EditProductCategory(data, productCategory);

            return TypedResults.Ok(productCategory);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return TypedResults.BadRequest("Unexpected error happened.");
        }
    }

    public static async Task<Results<Ok<string>, NotFound<string>, BadRequest<string>>> DeleteProductCategory(
        HttpContext httpCtx,
        [FromServices] IProductCategoryService productCategorySvc,
        [FromRoute] int categoryId)
    {
        try
        {
            var productCategory = await productCategorySvc.FindProductCategoryById(categoryId, false, false);
            if (productCategory is null)
            {
                return TypedResults.NotFound("Product Category did not found");
            }

            var result = await productCategorySvc.DeleteProductCategory(productCategory);
            if (!result)
            {
                return TypedResults.BadRequest("Deleting Product Category failed");
            }

            return TypedResults.Ok("Deleting Product Category success");
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return TypedResults.BadRequest("Unexpected error happened.");
        }
    }
}
