using ECommerce.Service;
using ECommerce.Types;
using ECommerce.Util;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Handler;

public static class OrderHandler
{
    public static async Task<Results<Ok<OrderTransaction>, NotFound<string>, BadRequest<string>>> PayingOrder(
        HttpContext httpCtx,
        [FromServices] IOrderService orderSvc,
        [FromServices] IOrderTransactionService orderTransactionSvc,
        [FromRoute] int orderId,
        [FromBody] PayingOrderDTO body)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var current_user = httpCtx.Items["current_user"] as Customer;
            var o = await orderSvc.FindMyOrderById(cts.Token, orderId, current_user!.Id, false);
            if (o is null)
            {
                return TypedResults.NotFound("Order did not found");
            }

            if (!o.OrderStatus.Equals(OrderStatus.WAITING_PAYMENT))
            {
                return TypedResults.BadRequest("Your order is not in WAITING PAYMENT status. Please check again");
            }

            var pm = body.paymentMethod.ToEnumOrThrow<PaymentMethod>();
            var ot = await orderTransactionSvc.PayingOrder(cts.Token, pm, o);

            return TypedResults.Ok(ot);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors ${err}");
            throw;
        }
    }

    public static async Task<Results<Created<Order>, BadRequest<string>>> CreateOrder(
        HttpContext httpCtx,
        [FromServices] IOrderService orderSvc,
        [FromServices] ICustomerCartService customerCartSvc)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(3));

            var current_user = httpCtx.Items["current_user"] as Customer;
            var myCart = await customerCartSvc.FindItemsInMyCart(cts.Token, current_user!.Id);
            if (!myCart.Any())
            {
                return TypedResults.BadRequest("Your cart is empty");
            }


            var o = await orderSvc.CreateOrder(cts.Token, current_user!.Id, myCart);

            return TypedResults.Created(httpCtx.Request.Path, o);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return TypedResults.BadRequest("Unexpected error happened.");
        }
    }

    public static async Task<Results<Ok<IEnumerable<Order>>, BadRequest<string>>> FindOrders(
        HttpContext httpCtx,
        [FromServices] IOrderService orderSvc,
        [FromQuery] string orderStatus = "ALL")
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var os = orderStatus.ToEnumOrDefault<OrderStatus>();
            var orders = await orderSvc.FindOrders(cts.Token, os, false);

            return TypedResults.Ok(orders);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return TypedResults.BadRequest("Unexpected error happened.");
        }
    }

    public static async Task<Results<Ok<Order>, BadRequest<string>>> FindOrderById(
        HttpContext httpCtx,
        [FromServices] IOrderService orderSvc,
        [FromRoute] int id)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
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
        [FromServices] IOrderService orderSvc,
        [FromQuery] string orderStatus = "ALL")
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var os = orderStatus.ToEnumOrDefault<OrderStatus>();
            var current_user = httpCtx.Items["current_user"] as Customer;
            var orders = await orderSvc.FindMyOrderHistories(cts.Token, current_user!.Id, os, false);

            return TypedResults.Ok(orders);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return TypedResults.BadRequest("Unexpected error happened.");
        }
    }

    public static async Task<Results<Ok<Order>, BadRequest<string>>> FindMyOrderById(
        HttpContext httpCtx,
        [FromServices] IOrderService orderSvc,
        [FromRoute] int id)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
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
