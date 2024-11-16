using ECommerce.Store;
using ECommerce.Types;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Service;

public interface ICustomerService
{
    Task<IEnumerable<Customer>> FindCustomers(bool track);
    Task<Customer?> FindCustomerById(int id, bool track);
    Task<Customer?> FindCustomerByNameOrEmail(string str, bool track);
    Task<Customer> CreateCustomer(CreateCustomerDTO data);
    Task<Customer> EditCustomer(EditCustomerDTO data, Customer c);
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

            return await query.FirstOrDefaultAsync(c => c.Id == id);
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            throw err;
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

            return await query.FirstOrDefaultAsync(c => c.Name.Equals(str) || c.Email.Equals(str));
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            throw err;
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
            throw err;
        }
    }

    public async Task<Customer> CreateCustomer(CreateCustomerDTO data)
    {
        try
        {
            var hashedPassword = this.passwordSvc.HashPassword(data.password);
            var c = new Customer()
            {
                Name = data.name,
                Email = data.email,
                Password = hashedPassword,
                Role = UserRoles.CUSTOMER,
                CreatedAt = DateTime.Now,
            };

            await this.ctx.Customers.AddAsync(c);
            await this.ctx.SaveChangesAsync();

            return c;
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            throw err;
        }
    }

    public async Task<Customer> EditCustomer(EditCustomerDTO data, Customer c)
    {
        try
        {
            c.Name = data.name;
            c.Email = data.email;
            c.Role = data.role;

            this.ctx.Customers.Update(c);
            await this.ctx.SaveChangesAsync();

            return c;
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"There are errors {err}");
            throw;
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
