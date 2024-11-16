using System.ComponentModel.DataAnnotations;

namespace ECommerce.Types;

public class OrderTransaction
{
    [Key]
    public int Id { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }
}

public enum PaymentMethod
{
    BANK,
    CREDIT_CARD,
    E_WALLET
}

public enum PaymentStatus
{
    PENDING,
    FAILED,
    SUCCESS
}
