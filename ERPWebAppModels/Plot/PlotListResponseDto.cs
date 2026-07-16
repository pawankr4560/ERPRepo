using ERPWebAppModels.Construction;

namespace ERPWebAppModels.Plot
{
    public class PlotListResponseDto
    {
        public List<PlotListingDto> Items { get; set; } = new List<PlotListingDto>(); 

        public PaginationDto Pagination { get; set; } = new PaginationDto();
    }
    public class PaginationDto
    {
        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int TotalPages { get; set; }
    }
}
