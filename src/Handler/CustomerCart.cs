using ECommerce.Service.Interface;
using ECommerce.Types;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Handler;

public static class CustomerCartHandler
{
    public static Results<Ok<IEnumerable<CustomerCartOverviewDTO>>, BadRequest<string>> FindMyCart(
        HttpContext httpCtx,
        [FromServices] ICustomerCartService customerCartSvc)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var current_user = httpCtx.Items["current_user"] as CustomerOverviewDTO;
            var myCart = customerCartSvc
                .FindItemsInMyCart(cts.Token,
                                   current_user!.id)
                .ToDTOS()
                .AsEnumerable();

            return TypedResults.Ok(myCart);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return TypedResults.BadRequest("Unexpected error happened.");
        }
    }

    public static async Task<Results<Ok<IEnumerable<CustomerCartOverviewDTO>>, NotFound<string>, BadRequest<string>>> AddItemToCart(
        HttpContext httpCtx,
        [FromServices] ICustomerCartService customerCartSvc,
        [FromServices] IProductService productService,
        [FromBody] CustomerCartDTO data)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var p = await productService.FindProductById(cts.Token, data.productId, false, false);
            if (p is null)
            {
                return TypedResults.NotFound("Product did not found");
            }

            if (p.Stock < data.quantity)
            {
                return TypedResults.BadRequest("Product stock is insufficient from your product quantity request");
            }

            var current_user = httpCtx.Items["current_user"] as CustomerOverviewDTO;
            var productIsExistInMyCart = await customerCartSvc.FindCartItemInMyCartByProductId(
                cts.Token,
                current_user!.id,
                data.productId,
                false,
                false);
            if (productIsExistInMyCart is not null)
            {
                return TypedResults.BadRequest("The Product is already in your cart.");
            }

            await customerCartSvc.AddItemToCart(cts.Token, current_user!.id, data);
            var myCart = customerCartSvc.FindItemsInMyCart(cts.Token, current_user!.id).ToDTOS().AsEnumerable();
            return TypedResults.Ok(myCart);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return TypedResults.BadRequest("Unexpected error happened.");
        }
    }

    public static async Task<Results<Ok<IEnumerable<CustomerCartOverviewDTO>>, NotFound<string>, BadRequest<string>>> RemoveItemToCart(
        HttpContext httpCtx,
        [FromServices] ICustomerCartService customerCartSvc,
        [FromRoute] int cartItemId)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var current_user = httpCtx.Items["current_user"] as CustomerOverviewDTO;
            var cc = await customerCartSvc.FindCartItemInMyCartById(cts.Token, current_user!.id, cartItemId, false, false);
            if (cc is null)
            {
                return TypedResults.NotFound("Cart Item did not found");
            }

            var myCart = customerCartSvc
                .FindItemsInMyCart(cts.Token,
                                   current_user!.id)
                .ToDTOS()
                .AsEnumerable();

            return TypedResults.Ok(myCart);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return TypedResults.BadRequest("Unexpected error happened.");
        }
    }

    public static async Task<Results<Ok<CustomerCartOverviewDTO>, Ok<string>, NotFound<string>, BadRequest<string>>> EditItemQuantity(
        HttpContext httpCtx,
        [FromServices] ICustomerCartService customerCartSvc,
        [FromRoute] int cartItemId,
        [FromBody] EditCustomerCartDTO body,
        [FromQuery] string changeItemQuantity = "INCREASE_OR_DECREASE")
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var current_user = httpCtx.Items["current_user"] as CustomerOverviewDTO;
            var cc = await customerCartSvc.FindCartItemInMyCartById(cts.Token, current_user!.id, cartItemId, false, true);
            if (cc is null)
            {
                return TypedResults.NotFound("Cart Item did not found");
            }

            if (changeItemQuantity.Equals("CHANGE", StringComparison.Ordinal))
            {
                cc = await customerCartSvc.EditItemQuantity(cts.Token, body.quantity, ChangeItemQuantity.CHANGE, cc);

                return TypedResults.Ok(cc.ToDTO());
            }

            if (cc.Product!.Stock < cc.Quantity + body.quantity)
            {
                return TypedResults.BadRequest("Your product quantity request is exceeded this product stock");
            }

            if (cc.Quantity + body.quantity == 0)
            {
                await customerCartSvc.RemoveItemFromCart(cts.Token, cc);

                return TypedResults.Ok("Cart item is deleted because you requested less than 1 quantity");
            }

            cc = await customerCartSvc.EditItemQuantity(cts.Token, body.quantity, ChangeItemQuantity.INCREASE_OR_DECREASE, cc);

            return TypedResults.Ok(cc.ToDTO());
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return TypedResults.BadRequest("Unexpected error happened.");
        }
    }

}
