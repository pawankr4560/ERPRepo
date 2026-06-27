using System.ComponentModel.DataAnnotations;

namespace WebApp.Model.Payment;

public class BookingPaymentChargeSettingRequest
{
    [Range(typeof(decimal), "0", "9999999999999999")]
    public decimal FixedCharge { get; set; }

    [Range(typeof(decimal), "0", "100")]
    public decimal PercentageCharge { get; set; }
}
