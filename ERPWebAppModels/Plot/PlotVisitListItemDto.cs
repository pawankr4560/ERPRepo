namespace ERPWebAppModels.Plot
{
    public class PlotVisitListItemDto
    {
        public Guid Id { get; set; }

        public Guid PlotId { get; set; }

        public string PropertyName { get; set; } = string.Empty;

        public DateOnly VisitDate { get; set; }

        public TimeOnly VisitTime { get; set; }

        public string Status { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;
    }
    public class PlotVisitListResponseDto
    {
        public List<PlotVisitListItemDto> Items { get; set; } = new();

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }
    }
}
