using ECommerce.Service;
using ECommerce.Types;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Handler;

public static class CustomerHandler
{
    public static async Task<Results<Ok<IEnumerable<Customer>>, BadRequest<string>>> FindCustomers(
            [FromServices] ICustomerService customerSvc)
    {
        try
        {
            var customers = await customerSvc.FindCustomers(false);

            return TypedResults.Ok(customers);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return TypedResults.BadRequest("Unexpected Error Happened.");
        }
    }

    public static async Task<Results<Ok<Customer>, NotFound<string>, BadRequest<string>>> FindCustomerById(
            [FromServices] ICustomerService customerSvc,
            int id)
    {
        try
        {
            var c = await customerSvc.FindCustomerById(id, false);
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

    public static async Task<Results<Ok<Customer>, NotFound<string>, BadRequest<string>>> FindCustomerByNameOrEmail(
        [FromServices] ICustomerService customerSvc,
        string str)
    {
        try
        {
            var c = await customerSvc.FindCustomerByNameOrEmail(str, false);
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
            [FromBody] CreateCustomerDTO data
            )
    {
        try
        {
            var c = await customerSvc.CreateCustomer(data);

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
            [FromBody] LoginDTO data
            )
    {
        try
        {
            var c = await customerSvc.FindCustomerByNameOrEmail(data.email, false);
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
        [FromBody] EditCustomerDTO data
        )
    {
        try
        {
            var current_user = httpCtx.Items["current_user"] as Customer;
            if (current_user is null)
            {
                return TypedResults.BadRequest("User is not authenticated. Please Login first");
            }

            var c = await customerSvc.EditCustomer(data, current_user);

            return TypedResults.Ok(c);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return TypedResults.BadRequest("Unexpected Error Happened.");
        }
    }

    public static async Task<Results<Ok<Customer>, NotFound<string>, BadRequest<string>>> UpgradeCustomerToAdmin(
        HttpContext httpCtx,
        [FromServices] ICustomerService customerSvc,
        [FromRoute] int customerId
        )
    {
        try
        {
            var c = await customerSvc.FindCustomerById(customerId, true);
            if (c is null)
            {
                return TypedResults.NotFound("User did not found");
            }

            c = await customerSvc.EditCustomer(new EditCustomerDTO(c.Name, c.Email, UserRoles.ADMIN), c);
            return TypedResults.Ok(c);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are error {err}");
            return TypedResults.BadRequest("Unexpected Error Happened.");
        }
    }


    public static async Task<Results<Ok<string>, NotFound<string>, BadRequest<string>, ForbidHttpResult>> DeleteCustomer(
            HttpContext httpCtx,
            [FromServices] ICustomerService customerSvc,
            [FromRoute] int customerId
            )
    {
        try
        {
            var current_user = httpCtx.Items["current_user"] as Customer;
            if (current_user is null)
            {
                return TypedResults.BadRequest("User is not authenticated. Please Login first");
            }

            if (current_user.Role != UserRoles.ADMIN)
            {
                return TypedResults.Forbid();
            }

            var c = await customerSvc.FindCustomerById(customerId, true);
            if (c is null)
            {
                return TypedResults.NotFound("User did not found");
            }

            var result = await customerSvc.DeleteCustomer(c);
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
