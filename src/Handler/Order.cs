using ECommerce.Service;
using ECommerce.Types;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Handler;

public static class OrderHandler
{
    public static async Task<Results<Created<Order>, BadRequest<string>>> CreateOrder(
        HttpContext httpCtx,
        CancellationToken ct,
        [FromServices] IOrderService orderSvc,
        [FromBody] CreateOrderDTO data)
    {
        try
        {
            if (!data.myCart.Any())
            {
                return TypedResults.BadRequest("Your cart is empty");
            }

            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(3));

            var current_user = httpCtx.Items["current_user"] as Customer;
            var o = await orderSvc.CreateOrder(cts.Token, current_user!.Id, data);

            return TypedResults.Created(httpCtx.Request.Path, o);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return TypedResults.BadRequest("Unexpected error happened.");
        }
    }

    public static async Task<Results<Ok<IEnumerable<Order>>, BadRequest<string>>> FindOrders(
        CancellationToken ct,
        [FromServices] IOrderService orderSvc)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var os = await orderSvc.FindOrders(cts.Token, false);

            return TypedResults.Ok(os);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return TypedResults.BadRequest("Unexpected error happened.");
        }
    }

    public static async Task<Results<Ok<Order>, BadRequest<string>>> FindOrderById(
        CancellationToken ct,
        [FromServices] IOrderService orderSvc,
        [FromRoute] int id)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var o = await orderSvc.FindOrderById(cts.Token, id, false);

            return TypedResults.Ok(o);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return TypedResults.BadRequest("Unexpected error happened.");
        }
    }

    public static async Task<Results<Ok<IEnumerable<Order>>, BadRequest<string>>> FindMyOrderHistories(
        HttpContext httpCtx,
        CancellationToken ct,
        [FromServices] IOrderService orderSvc)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var current_user = httpCtx.Items["current_user"] as Customer;
            var os = await orderSvc.FindMyOrderHistories(cts.Token, current_user!.Id, false);

            return TypedResults.Ok(os);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return TypedResults.BadRequest("Unexpected error happened.");
        }
    }

    public static async Task<Results<Ok<Order>, BadRequest<string>>> FindMyOrderById(
        HttpContext httpCtx,
        CancellationToken ct,
        [FromServices] IOrderService orderSvc,
        [FromRoute] int id)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var current_user = httpCtx.Items["current_user"] as Customer;
            var o = await orderSvc.FindMyOrderById(cts.Token, id, current_user!.Id, false);

            return TypedResults.Ok(o);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return TypedResults.BadRequest("Unexpected error happened.");
        }
    }
}
