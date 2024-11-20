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
    public CustomerService(
        ApplicationDbContext ctx,
        PasswordService passwordSvc,
        EmailBackgroundService emailSender)
    {
        this.ctx = ctx;
        this.passwordSvc = passwordSvc;
        this.emailBackgroundSvc = emailSender;
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
            Console.WriteLine($"There are errors {err}");
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
            Console.WriteLine($"There are errors {err}");
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
                    c => EF.Functions.Collate(c.Name, "SQL_Latin1_General_CP1_CS_AS").Equals(str) ||
                    EF.Functions.Collate(c.Email, "SQL_Latin1_General_CP1_CS_AS").Equals(str),
                    ct);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
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
            Console.WriteLine($"There are errors {err}");
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
            this.ctx.Customers.Add(c);

            if (addrData is not null)
            {
                var addr = new CustomerAddress
                {
                    CustomerId = c.Id,
                    Country = addrData.country,
                    State = addrData.state,
                    FullAddress = addrData.fullAddress,
                };

                this.ctx.CustomerAddresses.Add(addr);
            }

            await this.ctx.SaveChangesAsync(ct);

            await this.emailBackgroundSvc.QueueEmail(new sendEmailData(c.Email, "Email Verification", $"Verification your email {c.Email}"));

            await tx.CommitAsync(ct);
            return c;
        }
        catch (System.Exception err)
        {
            await tx.RollbackAsync(ct);
            Console.WriteLine($"There are errors {err}");
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

            this.ctx.Customers.Update(c);

            if (ca is not null)
            {
                ca.CustomerId = c.Id;
                this.ctx.CustomerAddresses.Update(ca);
                await this.ctx.SaveChangesAsync(ct);
            }

            await tx.CommitAsync(ct);
            return c;
        }
        catch (System.Exception err)
        {
            await tx.RollbackAsync(ct);
            Console.WriteLine($"There are errors {err}");
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
            Console.WriteLine($"There are errors {err}");
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
            Console.WriteLine($"There are errors {err}");
            throw;
        }
    }
}
