namespace WebApp.Model.Transaction
{
    public class LoanEMIScheduleDto
    {
        public int Id { get; set; }

        public int LoanId { get; set; }

        public int InstallmentNo { get; set; }

        public DateTime DueDate { get; set; }

        public decimal EMIAmount { get; set; }

        public decimal PrincipalAmount { get; set; }

        public decimal InterestAmount { get; set; }

        public decimal OutstandingBalance { get; set; }

        public bool IsPaid { get; set; }

        public DateTime? PaidDate { get; set; }
        public string? CustMobNo { get; set; }
        public string? GranterMobNo { get; set; }
        public string? GranterName { get; set; }
        public string? Relation { get; set; }
    }
}
