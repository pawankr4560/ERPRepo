namespace ERPWebAppModels.Construction
{
    public class ConstructionProductListDto
    {
        public List<ConstructionProductDto> Items { get; set; }
        = new List<ConstructionProductDto>();

        public PaginationDto Pagination { get; set; }
            = new PaginationDto();
    }
    public class PaginationDto
    {
        public int Page { get; set; }
        public int Limit { get; set; }
        public int Total { get; set; }
    }
}
