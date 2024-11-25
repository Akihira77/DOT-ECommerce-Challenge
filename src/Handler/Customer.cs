using ECommerce.Service.Interface;
using ECommerce.Types;
using ECommerce.Util;
using FluentValidation;
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
        [FromServices] IValidator<CreateCustomerDTO> customerValidator,
        [FromServices] IValidator<UpsertCustomerAddressDTO> customerAddressValidator,
        [FromServices] ICustomerService customerSvc,
        [FromBody] CreateCustomerAndCustomerAddressDTO body)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var validationResult = await customerValidator.ValidateAsync(body.custData, cts.Token);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            if (body.addrData is not null)
            {
                validationResult = await customerAddressValidator.ValidateAsync(body.addrData, cts.Token);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }
            }

            var c = await customerSvc.CreateCustomer(cts.Token, body.custData, body.addrData);

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
        [FromServices] IValidator<LoginDTO> validator,
        [FromServices] ICustomerService customerSvc,
        [FromServices] JwtService jwtSvc,
        [FromBody] LoginDTO body)
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

            var c = await customerSvc.FindCustomerByNameOrEmail(cts.Token, body.email, false);
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
        [FromServices] IValidator<EditCustomerDTO> customerValidator,
        [FromServices] IValidator<CustomerAddress> customerAddressValidator,
        [FromServices] ICustomerService customerSvc,
        [FromBody] EditCustomerAndCustomerAddressDTO body)
    {
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(httpCtx.RequestAborted);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var validationResult = await customerValidator.ValidateAsync(body.custData, cts.Token);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            if (body.addrData is not null)
            {
                validationResult = await customerAddressValidator.ValidateAsync(body.addrData, cts.Token);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }
            }

            var current_user = httpCtx.Items["current_user"] as CustomerOverviewDTO;
            var c = await customerSvc.FindCustomerById(cts.Token, current_user!.id, false);
            c = await customerSvc.EditCustomer(cts.Token, body.custData, c!, body.addrData);

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
        [FromServices] IValidator<UpsertCustomerAddressDTO> validator,
        [FromServices] ICustomerService customerSvc,
        [FromBody] UpsertCustomerAddressDTO body)
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
            var c = await customerSvc.AddCustomerAddress(cts.Token, current_user!.id, body);
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
