namespace WebApp.Model.Product
{
    public class ProductListItemDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public int SubcategoryId { get; set; }
        public string Categorie { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string SubcategoryName { get; set; } = string.Empty;
        public int UOMIndex { get; set; }
        public int UnitId { get; set; }
        public int LocationIndex { get; set; }
        public int StockQty { get; set; }
        public bool Status { get; set; }
        public bool IsActive { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
