using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using ECommerce.Middleware;
using ECommerce.Router;
using ECommerce.Service;
using ECommerce.Service.Interface;
using ECommerce.Store;
using ECommerce.Types;
using ECommerce.Util;
using FluentValidation;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

Log.Information($"Starting web application with PID: {Process.GetCurrentProcess().Id}");
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, cfg) =>
{
    cfg.WriteTo.Console();
    cfg.MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning);
    cfg.MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning);
    cfg.MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning);

});
builder.Services.AddHttpLogging(log =>
{
    log.LoggingFields = HttpLoggingFields.RequestMethod
                        | HttpLoggingFields.RequestPath
                        | HttpLoggingFields.RequestQuery
                        | HttpLoggingFields.Duration
                        | HttpLoggingFields.ResponseStatusCode;
    log.CombineLogs = true;
});

// builder.Services.AddDbContext<ApplicationDbContext>(o =>
// {
//     o.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer"));
// });
builder.Services.AddDbContextPool<ApplicationDbContext>(o =>
{
    o.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer"));
});
builder.Services.AddPooledDbContextFactory<ApplicationDbContext>(o =>
{
    o.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer"));
});

builder.Services.Configure<JsonOptions>(options =>
        options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddAntiforgery();
builder.Services.AddRateLimiter(conf =>
{
    conf.OnRejected = (context, cancellationToken) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = retryAfter.TotalSeconds.ToString();
        }

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.");

        return new ValueTask();
    };
    conf.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    conf.AddFixedWindowLimiter("fixed", o =>
    {
        o.PermitLimit = 10;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 0;
    });
});
builder.Services.AddProblemDetails(o =>
{
    o.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Instance = $"{ctx.HttpContext.Request.Method} {ctx.HttpContext.Request.Path}";
        ctx.ProblemDetails.Extensions.TryAdd("requestId", ctx.HttpContext.TraceIdentifier);

        var activity = ctx.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
        ctx.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
    };
});

builder.Services.AddSingleton(resolver =>
        resolver.GetRequiredService<IOptions<EmailConfiguration>>().Value);
builder.Services.Configure<EmailConfiguration>(
    builder.Configuration.GetSection("EtherealEmailConfiguration"));
builder.Services.AddSingleton<IEmailSender, EtherealEmailSender>();
// builder.Services.Configure<EmailConfiguration>(
//     builder.Configuration.GetSection("GmailConfiguration"));
// builder.Services.AddSingleton<IEmailSender, GmailEmailSender>();
builder.Services.AddSingleton<EmailBackgroundService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<EmailBackgroundService>());

builder.Services.AddSingleton<RestockProductBackgroundService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<RestockProductBackgroundService>());

builder.Services.AddScoped<IValidator<LoginDTO>, LoginValidator>();
builder.Services.AddScoped<IValidator<CreateCustomerDTO>, CreateCustomerValidator>();
builder.Services.AddScoped<IValidator<EditCustomerDTO>, EditCustomerValidator>();
builder.Services.AddScoped<IValidator<UpsertCustomerAddressDTO>, UpsertCustomerAddressValidator>();
builder.Services.AddScoped<IValidator<UpsertProductCategoryDTO>, UpsertProductCategoryValidator>();
builder.Services.AddScoped<IValidator<EditCustomerCartDTO>, EditCustomerCartValidator>();
builder.Services.AddScoped<IValidator<CustomerCartDTO>, CustomerCartValidator>();
builder.Services.AddScoped<IValidator<FindProductsQueryDTO>, FindProductQueryValidator>();
builder.Services.AddScoped<IValidator<CreateProductDTO>, CreateProductValidator>();
builder.Services.AddScoped<IValidator<EditProductDTO>, EditProductValidator>();
builder.Services.AddScoped<IValidator<CustomerAddress>, CustomerAddressValidator>();

builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IProductCategoryService, ProductCategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICustomerCartService, CustomerCartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderTransactionService, OrderTransactionService>();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseSerilogRequestLogging();

app.UseHttpLogging();
app.UseAntiforgery();
app.UseRateLimiter();

app.UseMiddleware<OperationCanceledMiddleware>();
app.UseMiddleware<ExtractUserDataFromCookie>();
app.MainRouter();

app.Run();
