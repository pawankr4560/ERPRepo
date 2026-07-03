namespace ERPWebAppModels.Transaction
{
    public class LoanDocumentsResponseDto
    {
        public string ApplicationId { get; set; }
        public List<LoanDocumentDto> Documents { get; set; } = new();
    }
   
}
