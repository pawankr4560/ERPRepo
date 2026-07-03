namespace ERPWebAppModels.Payment
{
    public class UserPaymentsResponseDto
    {
        public NextDuePaymentDto? NextDuePayment { get; set; }
        public List<PaymentHistoryDto> Payments { get; set; } = new();
    }
    public class NextDuePaymentDto
    {
        public string Title { get; set; } = "EMI payment";
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime DueDate { get; set; }
        public string ApplicationId { get; set; }
        public int LoanId { get; set; }
        public int ScheduleId { get; set; }
    }

    public class PaymentHistoryDto
    {
        public string Title { get; set; } = "EMI payment";
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Completed";
        public DateTime PaymentDate { get; set; }
        public string ApplicationId { get; set; }
        public string TransactionId { get; set; }
    }
}
