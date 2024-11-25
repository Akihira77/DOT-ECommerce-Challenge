using ECommerce.Service.Interface;
using ECommerce.Types;
using FluentValidation;
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
        [FromServices] IValidator<CustomerCartDTO> validator,
        [FromServices] ICustomerCartService customerCartSvc,
        [FromServices] IProductService productService,
        [FromBody] CustomerCartDTO body)
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

            var p = await productService.FindProductById(cts.Token, body.productId, false, false);
            if (p is null)
            {
                return new NotFoundError("Product is not found").ToResult();
            }

            if (p.Stock < body.quantity)
            {
                return new BadRequestError("Product stock is insufficient from your product quantity request").ToResult();
            }

            var current_user = httpCtx.Items["current_user"] as CustomerOverviewDTO;
            var productIsExistInMyCart = await customerCartSvc.FindCartItemInMyCartByProductId(
                cts.Token,
                current_user!.id,
                body.productId,
                false,
                false);
            if (productIsExistInMyCart is not null)
            {
                return new BadRequestError("The Product is already in your cart.").ToResult();
            }

            await customerCartSvc.AddItemToCart(cts.Token, current_user!.id, body);
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

            var result = await customerCartSvc.RemoveItemFromCart(cts.Token, cc);
            if (!result)
            {
                return new BadRequestError("Removing item in your cart is failed").ToResult();
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
        [FromServices] IValidator<EditCustomerCartDTO> validator,
        [FromServices] ICustomerCartService customerCartSvc,
        [FromRoute] int cartItemId,
        [FromBody] EditCustomerCartDTO body)
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


            var current_user = httpCtx.Items["current_user"] as CustomerOverviewDTO;
            var cc = await customerCartSvc.FindCartItemInMyCartById(cts.Token, current_user!.id, cartItemId, false, true);
            if (cc is null)
            {
                return new NotFoundError("Cart Item is not found").ToResult();
            }

            if (body.quantity <= 0 || body.quantity > Int32.MaxValue)
            {
                return new BadRequestError("Product quantity is invalid. Please input the correct number").ToResult();
            }

            cc = await customerCartSvc.EditItemQuantity(cts.Token, body.quantity, cc);

            if (cc.Product!.Stock < cc.Quantity + body.quantity)
            {
                return new BadRequestError("Your product quantity request is exceeded this product stock").ToResult();
            }

            cc = await customerCartSvc.EditItemQuantity(cts.Token, body.quantity, cc);

            return Results.Ok(cc.ToDTO());
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }

}
