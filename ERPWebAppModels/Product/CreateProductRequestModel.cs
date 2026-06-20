using Microsoft.AspNetCore.Http;

namespace WebApp.Model.Product
{
    public class CreateProductRequestModel
    {
        public string Name { get; set; } = string.Empty;
        public float Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public IFormFile? ProfileImage { get; set; }
        public string Categorie { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}
