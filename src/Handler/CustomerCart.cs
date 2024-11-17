using ECommerce.Service;
using ECommerce.Types;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Handler;

public static class CustomerCartHandler
{
    public static async Task<Results<Ok<IEnumerable<CustomerCart>>, BadRequest<string>>> FindMyCart(
        HttpContext httpCtx,
        CancellationToken ct,
        [FromServices] ICustomerCartService customerCartSvc)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var current_user = httpCtx.Items["current_user"] as Customer;
            var myCart = await customerCartSvc.FindItemsInMyCart(cts.Token, current_user!.Id);

            return TypedResults.Ok(myCart);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return TypedResults.BadRequest("Unexpected error happened.");
        }
    }

    public static async Task<Results<Ok<IEnumerable<CustomerCart>>, BadRequest<string>>> AddItemToCart(
        HttpContext httpCtx,
        CancellationToken ct,
        [FromServices] ICustomerCartService customerCartSvc,
        [FromBody] CustomerCartDTO data)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var current_user = httpCtx.Items["current_user"] as Customer;
            var myCart = await customerCartSvc.AddItemToCart(cts.Token, current_user!.Id, data);

            return TypedResults.Ok(myCart);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return TypedResults.BadRequest("Unexpected error happened.");
        }
    }

    public static async Task<Results<Ok<IEnumerable<CustomerCart>>, NotFound<string>, BadRequest<string>>> RemoveItemToCart(
        HttpContext httpCtx,
        CancellationToken ct,
        [FromServices] ICustomerCartService customerCartSvc,
        [FromRoute] int cartItemId)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var current_user = httpCtx.Items["current_user"] as Customer;
            var cc = await customerCartSvc.FindCartItemInMyCartById(cts.Token, current_user!.Id, cartItemId, false, false);
            if (cc is null)
            {
                return TypedResults.NotFound("Cart Item did not found");
            }

            var myCart = await customerCartSvc.RemoveItemFromCart(cts.Token, cc);

            return TypedResults.Ok(myCart);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return TypedResults.BadRequest("Unexpected error happened.");
        }
    }

    public static async Task<Results<Ok<CustomerCart>, Ok<string>, NotFound<string>, BadRequest<string>>> EditItemQuantity(
        HttpContext httpCtx,
        CancellationToken ct,
        [FromServices] ICustomerCartService customerCartSvc,
        [FromRoute] int cartItemId,
        [FromBody] int quantity,
        [FromQuery] string changeItemQuantity = "INCREASE_OR_DECREASE")
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var current_user = httpCtx.Items["current_user"] as Customer;
            var cc = await customerCartSvc.FindCartItemInMyCartById(cts.Token, current_user!.Id, cartItemId, false, true);
            if (cc is null)
            {
                return TypedResults.NotFound("Cart Item did not found");
            }

            if (changeItemQuantity.Equals("CHANGE", StringComparison.Ordinal))
            {
                cc = await customerCartSvc.EditItemQuantity(cts.Token, quantity, ChangeItemQuantity.CHANGE, cc);

                return TypedResults.Ok(cc);
            }

            if (cc.Product!.Stock < cc.Quantity + quantity)
            {
                return TypedResults.BadRequest("Your product quantity request is exceeded this product stock");
            }

            if (cc.Quantity + quantity == 0)
            {
                await customerCartSvc.RemoveItemFromCart(cts.Token, cc);

                return TypedResults.Ok("Cart item is deleted because you requested less than 1 quantity");
            }

            cc = await customerCartSvc.EditItemQuantity(cts.Token, quantity, ChangeItemQuantity.INCREASE_OR_DECREASE, cc);

            return TypedResults.Ok(cc);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return TypedResults.BadRequest("Unexpected error happened.");
        }
    }

}
