namespace WebApp.Model.Product
{
    public class UpdateProductModel
    {
        public string Name { get; set; } = string.Empty;
        public int Price { get; set; }
        public int StockQty { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Categorie { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public int SubcategoryId { get; set; }
        public int UOMIndex { get; set; }
        public int UnitId { get; set; }
        public int LocationIndex { get; set; }
        public bool Status { get; set; }
        public string Code { get; set; } = string.Empty;
        public int Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}

