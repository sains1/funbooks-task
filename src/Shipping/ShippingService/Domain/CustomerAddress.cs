namespace ShippingService.Domain;

public class CustomerAddress
{
    public required int CustomerId { get; set; }
    public required string AddressLine1 { get; set; }
    public required string AddressLine2 { get; set; }
    public required string PostCode { get; set; }
}