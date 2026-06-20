namespace WebApp.Model.Menu
{
    public class MenuTreeDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Route { get; set; }
        public string IconClass { get; set; }
        public int? ParentId { get; set; }
        public int OrderNumber { get; set; }
        public bool IsActive { get; set; }
        public List<MenuTreeDto> Children { get; set; } = new();
    }

}
