namespace ERPWebAppModels.Transaction
{
    public class LoanApplicationStatusDto
    {
        public string Id { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Status { get; set; } = string.Empty;

        public int StepsCompleted { get; set; }

        public List<LoanApplicationStageDto> Stages { get; set; } = new();
    }
}
