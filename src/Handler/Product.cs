using ECommerce.Service;
using ECommerce.Types;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Handler;

public static class ProductHandler
{
    public static async Task<Results<Ok<IEnumerable<Product>>, BadRequest<string>>> FindProducts(
        [FromServices] IProductService productSvc,
        [FromQuery] bool includeProductCategory = false)
    {
        try
        {
            var ps = await productSvc.FindProducts(false, includeProductCategory);

            return TypedResults.Ok(ps);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return TypedResults.BadRequest("Unexpected error happened.");
        }
    }

    public static async Task<Results<Ok<Product>, NotFound<string>, BadRequest<string>>> FindProductById(
        [FromServices] IProductService productSvc,
        [FromRoute] int id,
        [FromQuery] bool includeProductCategory = false)
    {
        try
        {
            var ps = await productSvc.FindProductById(id, false, includeProductCategory);
            if (ps is null)
            {
                return TypedResults.NotFound("Product did not found");
            }

            return TypedResults.Ok(ps);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return TypedResults.BadRequest("Unexpected error happened.");
        }
    }

    public static async Task<Results<Created<Product>, BadRequest<string>>> CreateProduct(
        HttpContext httpCtx,
        [FromServices] IProductService productSvc,
        [FromServices] IProductCategoryService productCategorySvc,
        [FromBody] CreateProductDTO data)
    {
        try
        {
            var pc = await productCategorySvc.FindProductCategoryById(data.productCategoryId, false, false);
            if (pc is null)
            {
                return TypedResults.BadRequest("Invalid Product Category");
            }

            var ps = await productSvc.CreateProduct(data, pc);
            if (ps is null)
            {
                return TypedResults.BadRequest("Failed Creating Product");
            }

            return TypedResults.Created(httpCtx.Request.Path, ps);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return TypedResults.BadRequest("Unexpected error happened.");
        }
    }

    public static async Task<Results<Ok<Product>, NotFound<string>, BadRequest<string>>> EditProduct(
        [FromServices] IProductService productSvc,
        [FromRoute] int productId,
        [FromBody] EditProductDTO data)
    {
        try
        {
            var ps = await productSvc.FindProductById(productId, false, true);
            if (ps is null)
            {
                return TypedResults.NotFound("Product did not found");
            }

            ps = await productSvc.EditProduct(data, ps);

            return TypedResults.Ok(ps);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return TypedResults.BadRequest("Unexpected error happened.");
        }
    }

    public static async Task<Results<Ok<string>, NotFound<string>, BadRequest<string>>> DeleteProduct(
        [FromServices] IProductService productSvc,
        [FromRoute] int productId)
    {
        try
        {
            var ps = await productSvc.FindProductById(productId, false, true);
            if (ps is null)
            {
                return TypedResults.NotFound("Product did not found");
            }

            var result = await productSvc.DeleteProduct(ps, ps.ProductCategory!);
            if (!result)
            {
                return TypedResults.BadRequest("Deleting Product Failed");
            }

            return TypedResults.Ok("Deleting Product Success");
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return TypedResults.BadRequest("Unexpected error happened.");
        }
    }

}
