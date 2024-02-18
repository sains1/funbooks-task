using System.ComponentModel.DataAnnotations;

namespace ShippingService.Options;

public class ShippingOptions
{
    public const string SectionName = nameof(ShippingOptions);

    [Required]
    public string WarehouseEmailRecipient { get; set; }
}