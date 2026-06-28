using Microsoft.AspNetCore.Http;

namespace WebApp.Model.Product
{
    public class CreateProductRequestModel
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public float Price { get; set; }
        public int StockQty { get; set; }
        public int UOMIndex { get; set; }
        public int LocationIndex { get; set; }
        public bool Status { get; set; }
        public string Description { get; set; } = string.Empty;
        public IFormFile? ProfileImage { get; set; }
        public string Image { get; set; } = string.Empty;
        public string Categorie { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}
