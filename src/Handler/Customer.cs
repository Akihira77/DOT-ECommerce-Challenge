using ECommerce.Service;
using ECommerce.Types;
using ECommerce.Util;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Handler;

public static class CustomerHandler
{
    public static async Task<Results<Ok<IEnumerable<Customer>>, BadRequest<string>>> FindCustomers(
        HttpContext httpCtx,
        [FromServices] ICustomerService customerSvc)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var customers = await customerSvc.FindCustomers(cts.Token, false);

            return TypedResults.Ok(customers);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return TypedResults.BadRequest("Unexpected Error Happened.");
        }
    }

    public static async Task<Results<Ok<Customer>, NotFound<string>, BadRequest<string>>> FindCustomerById(
        HttpContext httpCtx,
        [FromServices] ICustomerService customerSvc,
        [FromRoute] int id)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var c = await customerSvc.FindCustomerById(cts.Token, id, false);
            if (c is null)
            {
                return TypedResults.NotFound("User did not found");
            }

            return TypedResults.Ok(c);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return TypedResults.BadRequest("Unexpected Error Happened.");
        }
    }

    public static async Task<Results<Ok<Customer>, UnauthorizedHttpResult, BadRequest<string>>> FindMyCustomerInfo(
        HttpContext httpCtx,
        [FromServices] ICustomerService customerSvc)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var current_user = httpCtx.Items["current_user"] as Customer;
            var c = await customerSvc.FindCustomerById(cts.Token, current_user!.Id, false);

            return TypedResults.Ok(c);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return TypedResults.BadRequest("Unexpected Error Happened.");
        }
    }


    public static async Task<Results<Ok<Customer>, NotFound<string>, BadRequest<string>>> FindCustomerByNameOrEmail(
        HttpContext httpCtx,
        [FromServices] ICustomerService customerSvc,
        [FromRoute] string str)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var c = await customerSvc.FindCustomerByNameOrEmail(cts.Token, str, false);
            if (c is null)
            {
                return TypedResults.NotFound("User did not found");
            }

            return TypedResults.Ok(c);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return TypedResults.BadRequest("Unexpected Error Happened.");
        }
    }

    public static async Task<Results<Created<Customer>, BadRequest<string>>> CreateCustomer(
        HttpContext httpCtx,
        [FromServices] ICustomerService customerSvc,
        [FromBody] CreateCustomerAndCustomerAddressDTO data)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var c = await customerSvc.CreateCustomer(cts.Token, data.custData, data.addrData);

            return TypedResults.Created(httpCtx.Request.Path, c);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return TypedResults.BadRequest("Unexpected Error Happened.");
        }
    }

    public static async Task<Results<Ok<Customer>, NotFound<string>, BadRequest<string>>> Login(
        HttpContext httpCtx,
        [FromServices] ICustomerService customerSvc,
        [FromServices] JwtService jwtSvc,
        [FromBody] LoginDTO data)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var c = await customerSvc.FindCustomerByNameOrEmail(cts.Token, data.email, false);
            if (c is null)
            {
                return TypedResults.NotFound("User did not found");
            }

            var token = jwtSvc.GenerateJwtToken(c.Email);
            httpCtx.Response.Cookies.Append("token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Set to true if using HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddDays(1)
            });

            return TypedResults.Ok(c);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return TypedResults.BadRequest("Unexpected Error Happened.");
        }
    }

    public static async Task<Results<Ok<Customer>, NotFound<string>, BadRequest<string>>> EditCustomer(
        HttpContext httpCtx,
        [FromServices] ICustomerService customerSvc,
        [FromBody] EditCustomerAndCustomerAddressDTO data)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var current_user = httpCtx.Items["current_user"] as Customer;
            var c = await customerSvc.EditCustomer(cts.Token, data.custData, current_user!, data.addrData);
            return TypedResults.Ok(c);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return TypedResults.BadRequest("Unexpected Error Happened.");
        }
    }

    public static async Task<Results<Created<string>, NotFound<string>, BadRequest<string>>> AddCustomerAddress(
        HttpContext httpCtx,
        [FromServices] ICustomerService customerSvc,
        [FromBody] UpsertCustomerAddressDTO addrData)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var current_user = httpCtx.Items["current_user"] as Customer;
            var c = await customerSvc.AddCustomerAddress(cts.Token, current_user!.Id, addrData);
            if (!c)
            {
                return TypedResults.BadRequest("Adding New Customer Address is failed");
            }

            return TypedResults.Created(httpCtx.Request.Path, "New Customer Address is successfully added");
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return TypedResults.BadRequest("Unexpected Error Happened.");
        }
    }

    public static async Task<Results<Ok<Customer>, NotFound<string>, BadRequest<string>, StatusCodeHttpResult>> UpgradeCustomerToAdmin(
        HttpContext httpCtx,
        [FromServices] ICustomerService customerSvc,
        [FromRoute] int customerId)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var c = await customerSvc.FindCustomerById(cts.Token, customerId, true);
            if (c is null)
            {
                return TypedResults.NotFound("User did not found");
            }

            c = await customerSvc.EditCustomer(cts.Token, new EditCustomerDTO(c.Name, c.Email, UserRoles.ADMIN), c, null);
            return TypedResults.Ok(c);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return TypedResults.BadRequest("Unexpected Error Happened.");
        }
    }


    public static async Task<Results<Ok<string>, NotFound<string>, BadRequest<string>, StatusCodeHttpResult>> DeleteCustomer(
        HttpContext httpCtx,
        [FromServices] ICustomerService customerSvc,
        [FromRoute] int customerId)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var c = await customerSvc.FindCustomerById(cts.Token, customerId, true);
            if (c is null)
            {
                return TypedResults.NotFound("User did not found");
            }

            var result = await customerSvc.DeleteCustomer(cts.Token, c);
            if (result)
            {
                return TypedResults.Ok("Delete Customer success");
            }

            return TypedResults.BadRequest("Delete Customer failed");
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return TypedResults.BadRequest("Unexpected Error Happened.");
        }
    }
}
