namespace WebApp.Model.Order
{
    public class CreateOrderRequestModel
    {
        public string Id { get; set; }
        public string Image { get; set; }
        public string? Address { get; set; } = "King Street";
        public string Name { get; set; }
        public int Quantity { get; set; }
        public float Price { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow.Date;
    }
}
