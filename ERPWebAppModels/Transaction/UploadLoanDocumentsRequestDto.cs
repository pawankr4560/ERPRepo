namespace ERPWebAppModels.Transaction
{
    public class UploadLoanDocumentsRequestDto
    {
        public string ApplicationId { get; set; }

        public List<LoanDocumentDto> Documents { get; set; } = new();
    }

}
