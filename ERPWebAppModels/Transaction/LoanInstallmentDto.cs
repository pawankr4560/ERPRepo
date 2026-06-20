namespace WebApp.Model.Transaction
{
    public class LoanInstallmentDto
    {
        public int LoanId { get; set; }
        public int ScheduleId { get; set; }
        public int InstallmentNo { get; set; }
        public DateTime DueDate { get; set; }
        public decimal EMIAmount { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal OutstandingBalance { get; set; }
    }
}
