using ECommerce.Service.Interface;
using ECommerce.Types;
using ECommerce.Util;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Handler;

public static class OrderHandler
{
    public static async Task<IResult> OrderPaymentHook(
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
                return new NotFoundError("Order is not found").ToResult();
            }

            if (!o.OrderStatus.Equals(OrderStatus.WAITING_PAYMENT))
            {
                return new BadRequestError("Your order is not in WAITING PAYMENT status. Please check again").ToResult();
            }

            if (DateTime.Now.CompareTo(o.Deadline) > 0)
            {
                return new BadRequestError("Your payment deadline is expired. Try make an order again").ToResult();
            }

            //WARN: NOT HANDLING ANY PAYMENT STATUS CURRENTLY
            //JUST HANDLE THE PAYMENT_SUCCESS
            var pm = body.paymentMethod.ToEnumOrThrow<PaymentMethod>();
            var ot = await orderTransactionSvc.PayingOrder(cts.Token, pm, o);

            return Results.Ok(ot);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors ${err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }

    //NOTE: IN CASE USING STRIPE CUSTOMER WILL PERFORM CHECKOUT
    //AND SERVER WILL GENERATE CLIENT_SECRET FROM PAYMENT INTENT CREATION
    //CLIENT-SIDE CAN USE CLIENT_SECRET TO DIRECT TO STRIPE PAYMENT PAGE
    //SERVER WILL SET A WEBHOOK TO RECEIVE PAYMENT RESULT
    public static async Task<IResult> CreatePaymentIntent(
        HttpContext httpCtx,
        [FromServices] IOrderService orderSvc,
        [FromServices] ICustomerCartService customerCartSvc)
    {
        //TODO: PUSH TO ADMIN THERE IS CUSTOMER MAKE AN ORDER
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(3));

            var current_user = httpCtx.Items["current_user"] as CustomerOverviewDTO;
            var myCart = customerCartSvc.FindItemsInMyCart(cts.Token, current_user!.id);
            if (!myCart.Any())
            {
                return new NotFoundError("Your cart is empty").ToResult();
            }

            var o = await orderSvc.CreateOrder(cts.Token, current_user, myCart);

            return Results.Created(httpCtx.Request.Path, o);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }

    public static async Task<IResult> FindOrders(
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

            return Results.Ok(orders);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }

    public static async Task<IResult> FindOrderById(
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
                return new NotFoundError($"Order {id} did not found").ToResult();
            }

            return Results.Ok(o);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }

    public static async Task<IResult> FindMyOrderHistories(
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

            return Results.Ok(orders);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }

    public static async Task<IResult> FindMyOrderById(
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
                return new NotFoundError($"Order {id} did not found").ToResult();
            }

            return Results.Ok(o);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }

    public static async Task<IResult> UpdateOrderStatus(
        HttpContext httpCtx,
        [FromServices] IOrderService orderSvc,
        [FromRoute] int orderId,
        [FromBody] UpdateOrderDTO body)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var o = await orderSvc.FindOrderById(cts.Token, orderId, false);
            if (o is null)
            {
                return new NotFoundError($"Order {orderId} did not found").ToResult();
            }

            var updatedOrder = await orderSvc.UpdateOrderStatus(cts.Token, o, body);
            return Results.Ok(updatedOrder);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }

    public static async Task<IResult> GenerateReport(
        HttpContext httpCtx,
        [FromServices] IOrderService orderSvc,
        [FromQuery] DateTime endDate,
        [FromQuery] DateTime? startDate = null)
    {
        try
        {
            if (startDate is not null && startDate.Value.CompareTo(endDate) > 0)
            {
                return new BadRequestError("Start Date is greater than End Date").ToResult();
            }

            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(3));

            var current_user = httpCtx.Items["current_user"] as CustomerOverviewDTO;
            await orderSvc.GenerateReport(cts.Token, current_user!, startDate ?? DateTime.Now, endDate);

            return Results.NoContent();
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }
}
