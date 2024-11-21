using ECommerce.Service;
using ECommerce.Service.Interface;
using ECommerce.Types;
using ECommerce.Util;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Handler;

public static class OrderHandler
{
    public static async Task<Results<Ok<OrderTransaction>, NotFound<string>, BadRequest<string>>> OrderPaymentHook(
        HttpContext httpCtx,
        [FromServices] IOrderService orderSvc,
        [FromServices] IOrderTransactionService orderTransactionSvc,
        [FromRoute] int orderId,
        [FromBody] PayingOrderDTO body)
    {
        //TODO: PUSH TO ADMIN THERE IS CUSTOMER PAYING THEIR ORDER
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var current_user = httpCtx.Items["current_user"] as CustomerOverviewDTO;
            var o = await orderSvc.FindMyOrderById(cts.Token, orderId, current_user!.id, false);
            if (o is null)
            {
                return TypedResults.NotFound("Order did not found");
            }

            if (!o.OrderStatus.Equals(OrderStatus.WAITING_PAYMENT))
            {
                return TypedResults.BadRequest("Your order is not in WAITING PAYMENT status. Please check again");
            }

            //WARN: NOT HANDLING ANY PAYMENT STATUS CURRENTLY
            //JUST HANDLE THE PAYMENT_SUCCESS
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

    //NOTE: IN CASE USING STRIPE CUSTOMER WILL PERFORM CHECKOUT
    //AND SERVER WILL GENERATE CLIENT_SECRET FROM PAYMENT INTENT CREATION
    //CLIENT-SIDE CAN USE CLIENT_SECRET TO DIRECT TO STRIPE PAYMENT PAGE
    //SERVER WILL SET A WEBHOOK TO RECEIVE PAYMENT RESULT
    public static async Task<Results<Created<Order>, BadRequest<string>>> CreatePaymentIntent(
        HttpContext httpCtx,
        [FromServices] IOrderService orderSvc,
        [FromServices] ICustomerCartService customerCartSvc)
    {
        //TODO: SEND ORDER RECEIPT EMAIL TO CUSTOMER
        //TODO: PUSH TO ADMIN THERE IS CUSTOMER MAKE AN ORDER
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(3));

            var current_user = httpCtx.Items["current_user"] as CustomerOverviewDTO;
            var myCart = customerCartSvc.FindItemsInMyCart(cts.Token, current_user!.id);
            if (!myCart.Any())
            {
                return TypedResults.BadRequest("Your cart is empty");
            }

            var o = await orderSvc.CreateOrder(cts.Token, current_user, myCart);

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

    public static async Task<Results<Ok<Order>, NotFound<string>, BadRequest<string>>> FindOrderById(
        HttpContext httpCtx,
        [FromServices] IOrderService orderSvc,
        [FromRoute] int id)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var o = await orderSvc.FindOrderById(cts.Token, id, false);
            if (o is null)
            {
                return TypedResults.NotFound($"Order {id} did not found");
            }

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
            var current_user = httpCtx.Items["current_user"] as CustomerOverviewDTO;
            var orders = await orderSvc.FindMyOrderHistories(cts.Token, current_user!.id, os, false);

            return TypedResults.Ok(orders);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return TypedResults.BadRequest("Unexpected error happened.");
        }
    }

    public static async Task<Results<Ok<Order>, NotFound<string>, BadRequest<string>>> FindMyOrderById(
        HttpContext httpCtx,
        [FromServices] IOrderService orderSvc,
        [FromRoute] int id)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var current_user = httpCtx.Items["current_user"] as CustomerOverviewDTO;
            var o = await orderSvc.FindMyOrderById(cts.Token, id, current_user!.id, false);
            if (o is null)
            {
                return TypedResults.NotFound($"Order {id} did not found");
            }

            return TypedResults.Ok(o);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return TypedResults.BadRequest("Unexpected error happened.");
        }
    }

    public static async Task<Results<Ok<Order>, NotFound<string>, BadRequest<string>>> UpdateOrderStatus(
        HttpContext httpCtx,
        [FromServices] IOrderService orderSvc,
        [FromRoute] int orderId,
        [FromBody] UpdateOrderDTO body)
    {
        //TODO: SEND EMAIL TO CUSTOMER FOR THEIR UPDATED ORDER FROM ADMIN
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var o = await orderSvc.FindOrderById(cts.Token, orderId, false);
            if (o is null)
            {
                return TypedResults.NotFound($"Order {orderId} did not found");
            }

            var updatedOrder = await orderSvc.UpdateOrderStatus(cts.Token, o, body);
            return TypedResults.Ok(updatedOrder);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return TypedResults.BadRequest("Unexpected error happened.");
        }
    }
}
