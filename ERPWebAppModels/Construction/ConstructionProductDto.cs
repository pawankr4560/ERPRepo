namespace ERPWebAppModels.Construction
{
    public class ConstructionProductDto
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; } 
        public string CategoryName { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public string Unit { get; set; } = string.Empty;
        public int UnitIndex { get; set; }
    }
}
