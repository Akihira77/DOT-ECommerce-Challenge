using ECommerce.Service.Interface;
using ECommerce.Store;
using ECommerce.Types;
using ECommerce.Util;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Service;

public class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext ctx;
    private readonly PasswordService passwordSvc;
    private readonly EmailBackgroundService emailBackgroundSvc;
    private readonly ILogger<CustomerService> logger;

    public CustomerService(
        ApplicationDbContext ctx,
        PasswordService passwordSvc,
        EmailBackgroundService emailSender,
        ILogger<CustomerService> logger)
    {
        this.ctx = ctx;
        this.passwordSvc = passwordSvc;
        this.emailBackgroundSvc = emailSender;
        this.logger = logger;
    }

    public async Task<Customer?> FindCustomerById(
        CancellationToken ct,
        int id,
        bool track)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var query = ctx.Customers.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            return await query.
                Include(c => c.CustomerAddresses).
                FirstOrDefaultAsync(c => c.Id.Equals(id), ct);
        }
        catch (System.Exception err)
        {
            this.logger.LogError($"Error in {err.Source} - {err.Message}");
            throw;
        }
    }

    public async Task<CustomerAddress?> FindCustomerAddressByCustomerId(
        CancellationToken ct,
        int id,
        bool track)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var query = ctx.CustomerAddresses.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(c => c.CustomerId.Equals(id), ct);
        }
        catch (System.Exception err)
        {
            this.logger.LogError($"Error in {err.Source} - {err.Message}");
            return null;
        }
    }


    public async Task<Customer?> FindCustomerByNameOrEmail(
        CancellationToken ct,
        string str,
        bool track)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var query = ctx.Customers.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(
                    c => EF.Functions.Collate(c.Name, "SQL_Latin1_General_CP1_CS_AS").StartsWith(str) ||
                    EF.Functions.Collate(c.Email, "SQL_Latin1_General_CP1_CS_AS").StartsWith(str),
                    ct);
        }
        catch (System.Exception err)
        {
            this.logger.LogError($"Error in {err.Source} - {err.Message}");
            return null;
        }
    }

    public IQueryable<Customer> FindCustomers(
        CancellationToken ct,
        bool track)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var query = ctx.Customers.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            return query;
        }
        catch (System.Exception err)
        {
            this.logger.LogError($"Error in {err.Source} - {err.Message}");
            throw;
        }
    }

    public async Task<Customer> CreateCustomer(
        CancellationToken ct,
        CreateCustomerDTO custData,
        UpsertCustomerAddressDTO? addrData)
    {
        using var tx = this.ctx.Database.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
        try
        {
            ct.ThrowIfCancellationRequested();

            var hashedPassword = this.passwordSvc.HashPassword(custData.password);
            var c = new Customer()
            {
                Name = custData.name,
                Email = custData.email,
                Password = hashedPassword,
                Role = UserRoles.CUSTOMER,
                CreatedAt = DateTime.Now,
            };

            if (addrData is not null)
            {
                var addr = new CustomerAddress
                {
                    CustomerId = c.Id,
                    Country = addrData.country,
                    State = addrData.state,
                    FullAddress = addrData.fullAddress,
                };

                c.CustomerAddresses.Add(addr);
            }

            await this.ctx.Customers.AddAsync(c, ct);
            await this.ctx.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return c;
        }
        catch (System.Exception err)
        {
            await tx.RollbackAsync(ct);
            this.logger.LogError($"Error in {err.Source} - {err.Message}");
            throw;
        }
    }

    public async Task<Customer> EditCustomer(
        CancellationToken ct,
        EditCustomerDTO custData,
        Customer c,
        CustomerAddress? ca)
    {
        using var tx = this.ctx.Database.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
        try
        {
            ct.ThrowIfCancellationRequested();

            c.Name = custData.name;
            c.Email = custData.email;
            c.Role = custData.role;

            if (ca is not null)
            {
                ca.CustomerId = c.Id;

                var existingAddress = c.CustomerAddresses.FirstOrDefault(ca => ca.Id.Equals(ca.Id));
                if (existingAddress is not null)
                {
                    existingAddress.Country = ca.Country;
                    existingAddress.State = ca.State;
                    existingAddress.FullAddress = ca.FullAddress;
                    this.ctx.CustomerAddresses.Update(existingAddress);
                }
                else
                {
                    await this.ctx.CustomerAddresses.AddAsync(ca, ct);
                    c.CustomerAddresses.Add(ca);
                }
            }

            this.ctx.Customers.Update(c);
            await this.ctx.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return c;
        }
        catch (System.Exception err)
        {
            await tx.RollbackAsync(ct);
            this.logger.LogError($"Error in {err.Source} - {err.Message}");
            throw;
        }
    }

    public async Task<bool> AddCustomerAddress(
        CancellationToken ct,
        int customerId,
        UpsertCustomerAddressDTO addrData)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var ca = new CustomerAddress
            {
                Country = addrData.country,
                State = addrData.state,
                FullAddress = addrData.fullAddress,
                CustomerId = customerId,
            };

            this.ctx.CustomerAddresses.Add(ca);
            return await this.ctx.SaveChangesAsync(ct) > 0;
        }
        catch (System.Exception err)
        {
            this.logger.LogError($"Error in {err.Source} - {err.Message}");
            throw;
        }
    }

    public async Task<bool> DeleteCustomer(
        CancellationToken ct,
        Customer c)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            ctx.Customers.Remove(c);

            return await ctx.SaveChangesAsync(ct) > 0;
        }
        catch (System.Exception err)
        {
            this.logger.LogError($"Error in {err.Source} - {err.Message}");
            throw;
        }
    }
}
