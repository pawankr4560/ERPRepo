using System.ComponentModel.DataAnnotations;

namespace WebApp.Model.Order;

public class SalesOrderItemRequest
{
    [Required]
    public string ProductId { get; set; } = string.Empty;

    [Range(1, 999999)]
    public int Quantity { get; set; }
}

public class SalesOrderCheckoutRequest
{
    [Required]
    public string OrderType { get; set; } = "Delivery";

    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    [MinLength(1)]
    public List<SalesOrderItemRequest> Items { get; set; } = [];
}

public class SalesOrderRazorpayOrderResponse
{
    public string KeyId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal ServiceFeeAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public long AmountPaise { get; set; }
    public string Currency { get; set; } = "INR";
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
}

public class SalesOrderVerifyRequest : SalesOrderCheckoutRequest
{
    [Required]
    public string RazorpayOrderId { get; set; } = string.Empty;

    [Required]
    public string RazorpayPaymentId { get; set; } = string.Empty;

    [Required]
    public string RazorpaySignature { get; set; } = string.Empty;
}

public class SalesOrderVerifyResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
