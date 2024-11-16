using ECommerce;
using ECommerce.Service;
using ECommerce.Store;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpLogging(log =>
{
    log.LoggingFields = HttpLoggingFields.All;
});

builder.Services.AddDbContext<ApplicationDbContext>(o =>
{
    o.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer"));
});

builder.Services.AddAntiforgery();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IProductCategoryService, ProductCategoryService>();

var app = builder.Build();

app.UseHttpLogging();
app.UseAntiforgery();

app.UseMiddleware<ExtractUserDataFromCookie>();
app.MainRouter();

app.Run();
