using ECommerce.Store;
using ECommerce.Types;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Service;

public interface ICustomerService
{
    Task<IEnumerable<Customer>> FindCustomers(bool track);
    Task<Customer?> FindCustomerById(int id, bool track);
    Task<CustomerAddress?> FindCustomerAddressByCustomerId(int id, bool track);
    Task<Customer?> FindCustomerByNameOrEmail(string str, bool track);
    Task<Customer> CreateCustomer(CreateCustomerDTO custData,
        UpsertCustomerAddressDTO? addrData);
    Task<Customer> EditCustomer(EditCustomerDTO custData,
        Customer c,
        CustomerAddress? ca);
    Task<bool> AddCustomerAddress(int customerId, UpsertCustomerAddressDTO addrData);
    Task<bool> DeleteCustomer(Customer c);
}

public class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext ctx;
    private readonly PasswordService passwordSvc;
    public CustomerService(ApplicationDbContext ctx, PasswordService passwordSvc)
    {
        this.ctx = ctx;
        this.passwordSvc = passwordSvc;
    }

    public async Task<Customer?> FindCustomerById(int id, bool track)
    {
        try
        {
            var query = ctx.Customers.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            return await query.
                Include(c => c.CustomerAddresses).
                FirstOrDefaultAsync(c => c.Id.Equals(id));
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return null;
        }
    }

    public async Task<CustomerAddress?> FindCustomerAddressByCustomerId(int id, bool track)
    {
        try
        {
            var query = ctx.CustomerAddresses.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(c => c.CustomerId.Equals(id));
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return null;
        }
    }


    public async Task<Customer?> FindCustomerByNameOrEmail(string str, bool track)
    {
        try
        {
            var query = ctx.Customers.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(
                    c => EF.Functions.Collate(c.Name, "SQL_Latin1_General_CP1_CS_AS").Equals(str) ||
                    EF.Functions.Collate(c.Email, "SQL_Latin1_General_CP1_CS_AS").Equals(str));
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return null;
        }
    }

    public async Task<IEnumerable<Customer>> FindCustomers(bool track)
    {
        try
        {
            var query = ctx.Customers.AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            return await query.ToListAsync();
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return [];
        }
    }

    public async Task<Customer> CreateCustomer(CreateCustomerDTO custData,
        UpsertCustomerAddressDTO? addrData)
    {
        using var tx = this.ctx.Database.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
        try
        {
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
            await this.ctx.SaveChangesAsync();

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
                await this.ctx.SaveChangesAsync();
            }

            tx.Commit();
            return c;
        }
        catch (System.Exception err)
        {
            tx.Rollback();
            Console.WriteLine($"There are errors {err}");
            return null;
        }
    }

    public async Task<Customer> EditCustomer(EditCustomerDTO custData,
        Customer c,
        CustomerAddress? ca)
    {
        using var tx = this.ctx.Database.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
        try
        {

            c.Name = custData.name;
            c.Email = custData.email;
            c.Role = custData.role;

            this.ctx.Customers.Update(c);
            await this.ctx.SaveChangesAsync();

            if (ca is not null)
            {
                ca.CustomerId = c.Id;
                this.ctx.CustomerAddresses.Update(ca);
                await this.ctx.SaveChangesAsync();
            }

            tx.Commit();
            return c;
        }
        catch (System.Exception err)
        {
            tx.Rollback();
            Console.WriteLine($"There are errors {err}");
            return null;
        }
    }

    public async Task<bool> AddCustomerAddress(int customerId, UpsertCustomerAddressDTO addrData)
    {
        try
        {
            var ca = new CustomerAddress
            {
                Country = addrData.country,
                State = addrData.state,
                FullAddress = addrData.fullAddress,
                CustomerId = customerId,
            };

            this.ctx.CustomerAddresses.Add(ca);
            return await this.ctx.SaveChangesAsync() > 0;
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            return false;
        }
    }

    public async Task<bool> DeleteCustomer(Customer c)
    {
        try
        {
            ctx.Customers.Remove(c);

            return await ctx.SaveChangesAsync() > 0;
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            throw err;
        }
    }
}
