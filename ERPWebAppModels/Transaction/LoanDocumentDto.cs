namespace ERPWebAppModels.Transaction
{
    public class LoanDocumentDto
    {
        public int DocumentId { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public DateTime? UploadedAt { get; set; }
        public string? FileName { get; set; }
    }
}
