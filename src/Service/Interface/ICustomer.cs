using ECommerce.Types;

namespace ECommerce.Service;

public interface ICustomerService
{
    IQueryable<Customer> FindCustomers(
        CancellationToken ct,
        bool track);
    Task<Customer?> FindCustomerById(
        CancellationToken ct,
        int id,
        bool track);
    Task<CustomerAddress?> FindCustomerAddressByCustomerId(
        CancellationToken ct,
        int id,
        bool track);
    Task<Customer?> FindCustomerByNameOrEmail(
        CancellationToken ct,
        string str,
        bool track);
    Task<Customer> CreateCustomer(
        CancellationToken ct,
        CreateCustomerDTO custData,
        UpsertCustomerAddressDTO? addrData);
    Task<Customer> EditCustomer(
        CancellationToken ct,
        EditCustomerDTO custData,
        Customer c,
        CustomerAddress? ca);
    Task<bool> AddCustomerAddress(
        CancellationToken ct,
        int customerId,
        UpsertCustomerAddressDTO addrData);
    Task<bool> DeleteCustomer(
        CancellationToken ct,
        Customer c);
}

