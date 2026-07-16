namespace ERPWebAppModels.Plot
{
    public class SavePlotResponseDto
    {
        public Guid PlotId { get; set; }

        public bool IsSaved { get; set; }

        public DateTime SavedAt { get; set; }
    }
}
