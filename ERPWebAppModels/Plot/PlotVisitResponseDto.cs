namespace ERPWebAppModels.Plot
{
    public class PlotVisitResponseDto
    {
        public Guid Id { get; set; }

        public Guid PlotId { get; set; }

        public string PropertyName { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string MobileNumber { get; set; } = string.Empty;

        public DateOnly VisitDate { get; set; }

        public TimeOnly VisitTime { get; set; }

        public string? Remarks { get; set; }

        public string Status { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
