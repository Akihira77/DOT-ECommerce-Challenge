using ECommerce.Service.Interface;
using ECommerce.Types;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Handler;

public static class CustomerCartHandler
{
    public static IResult FindMyCart(
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

            return Results.Ok(myCart);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }

    public static async Task<IResult> AddItemToCart(
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
                return new NotFoundError("Product is not found").ToResult();
            }

            if (p.Stock < data.quantity)
            {
                return new BadRequestError("Product stock is insufficient from your product quantity request").ToResult();
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
                return new BadRequestError("The Product is already in your cart.").ToResult();
            }

            await customerCartSvc.AddItemToCart(cts.Token, current_user!.id, data);
            var myCart = customerCartSvc.FindItemsInMyCart(cts.Token, current_user!.id).ToDTOS().AsEnumerable();
            return Results.Ok(myCart);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }

    public static async Task<IResult> RemoveItemToCart(
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
                return new NotFoundError("Cart Item did not found").ToResult();
            }

            var myCart = customerCartSvc
                .FindItemsInMyCart(cts.Token,
                                   current_user!.id)
                .ToDTOS()
                .AsEnumerable();

            return Results.Ok(myCart);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }

    public static async Task<IResult> EditItemQuantity(
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
                return new NotFoundError("Cart Item is not found").ToResult();
            }

            if (changeItemQuantity.Equals("CHANGE", StringComparison.Ordinal))
            {
                cc = await customerCartSvc.EditItemQuantity(cts.Token, body.quantity, ChangeItemQuantity.CHANGE, cc);

                return Results.Ok(cc.ToDTO());
            }

            if (cc.Product!.Stock < cc.Quantity + body.quantity)
            {
                return new BadRequestError("Your product quantity request is exceeded this product stock").ToResult();
            }

            if (cc.Quantity + body.quantity == 0)
            {
                return new BadRequestError("Cart item quantity cannot less than 1. Please check again").ToResult();
            }

            cc = await customerCartSvc.EditItemQuantity(cts.Token, body.quantity, ChangeItemQuantity.INCREASE_OR_DECREASE, cc);

            return Results.Ok(cc.ToDTO());
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }

}
