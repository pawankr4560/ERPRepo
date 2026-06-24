namespace WebApp.Model.Menu
{
    public class MenuItemDto
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Route { get; set; }
        public string IconClass { get; set; } = string.Empty;
        public int OrderNumber { get; set; }
        public bool IsActive { get; set; }
    }

}
