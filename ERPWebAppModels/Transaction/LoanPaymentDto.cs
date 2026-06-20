namespace WebApp.Model.Transaction
{
    public class LoanPaymentDto
    {
        public int Id { get; set; }
        public int LoanId { get; set; }
        public int ScheduleId { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string? PaymentMode { get; set; }
        public string PaymentStatus { get; set; } = "Success";
        public string? Remarks { get; set; }
    }
}
