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
    BANK = 0,
    CREDIT_CARD = 1,
    E_WALLET = 2
}

public enum PaymentStatus
{
    PENDING = 0,
    FAILED = 1,
    SUCCESS = 2
}

public record PayingOrderDTO(string paymentMethod);
