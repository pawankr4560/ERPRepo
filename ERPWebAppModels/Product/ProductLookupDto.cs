namespace WebApp.Model.Product
{
    public class CategoryLookupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class SubCategoryLookupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
    }
}
