namespace WebApp.Model.Product
{
    public class UpdateProductModel
    {
        public string Name { get; set; } = string.Empty;
        public int Price { get; set; }
        public int StockQty { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Categorie { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}
