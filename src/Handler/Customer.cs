using ECommerce.Service.Interface;
using ECommerce.Types;
using ECommerce.Util;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Handler;

public static class CustomerHandler
{
    public static IResult FindCustomers(
        HttpContext httpCtx,
        [FromServices] ICustomerService customerSvc)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var queryable = customerSvc.FindCustomers(cts.Token, false);
            var customers = queryable
                .ToCustomersOverviewDTO()
                .AsEnumerable();

            return Results.Ok(customers);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }

    public static async Task<IResult> FindCustomerById(
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
                return new NotFoundError("Customer data is not found").ToResult();
            }

            return Results.Ok(c.ToCustomerOverviewDTO());
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }

    public static async Task<IResult> FindMyCustomerInfo(
        HttpContext httpCtx,
        [FromServices] ICustomerService customerSvc)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var current_user = httpCtx.Items["current_user"] as CustomerOverviewDTO;
            var c = await customerSvc.FindCustomerById(cts.Token, current_user!.id, false);

            return Results.Ok(c);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }


    public static async Task<IResult> FindCustomerByNameOrEmail(
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
                return new NotFoundError("Customer data is not found").ToResult();
            }

            return Results.Ok(c);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }

    public static async Task<IResult> CreateCustomer(
        HttpContext httpCtx,
        [FromServices] ICustomerService customerSvc,
        [FromBody] CreateCustomerAndCustomerAddressDTO data)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var c = await customerSvc.CreateCustomer(cts.Token, data.custData, data.addrData);

            return Results.Created(httpCtx.Request.Path, c);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }

    public static async Task<IResult> Login(
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
                return new NotFoundError("Customer data is not found").ToResult();
            }

            var token = jwtSvc.GenerateJwtToken(c.Email);
            httpCtx.Response.Cookies.Append("token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Set to true if using HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddDays(1)
            });

            return Results.Ok(c.ToCustomerOverviewDTO());
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }

    public static async Task<IResult> EditCustomer(
        HttpContext httpCtx,
        [FromServices] ICustomerService customerSvc,
        [FromBody] EditCustomerAndCustomerAddressDTO data)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var current_user = httpCtx.Items["current_user"] as CustomerOverviewDTO;
            var c = await customerSvc.FindCustomerById(cts.Token, current_user!.id, false);
            c = await customerSvc.EditCustomer(cts.Token, data.custData, c!, data.addrData);

            return Results.Ok(c);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }

    public static async Task<IResult> AddCustomerAddress(
        HttpContext httpCtx,
        [FromServices] ICustomerService customerSvc,
        [FromBody] UpsertCustomerAddressDTO addrData)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var current_user = httpCtx.Items["current_user"] as CustomerOverviewDTO;
            var c = await customerSvc.AddCustomerAddress(cts.Token, current_user!.id, addrData);
            if (!c)
            {
                return new BadRequestError("Adding new customer address error").ToResult();
            }

            return Results.Created(httpCtx.Request.Path, "New Customer Address is successfully added");
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }

    public static async Task<IResult> UpgradeCustomerToAdmin(
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
                return new NotFoundError("Customer data is not found").ToResult();
            }

            c = await customerSvc.EditCustomer(cts.Token, new EditCustomerDTO(c.Name, c.Email, UserRoles.ADMIN), c, null);
            return Results.Ok(c);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }


    public static async Task<IResult> DeleteCustomer(
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
                return new NotFoundError("Customer data is not found").ToResult();
            }

            var result = await customerSvc.DeleteCustomer(cts.Token, c);
            if (!result)
            {
                return new BadRequestError("Failed deleting customer data").ToResult();
            }

            return Results.Ok("Success deleting");
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return new InternalServerError("Unexpected error happened.").ToResult();
        }
    }
}
