using ShippingService.Domain;

namespace ShippingService.Application.ProductShipping;

public interface ICustomerAddressRepository
{
    Task<CustomerAddress> GetCustomerAddressAsync(int customerId);
}