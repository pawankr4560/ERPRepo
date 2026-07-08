namespace ERPWebAppData.Entity
{
    public class ConstructionQuote
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public decimal Quantity { get; set; }
        public int UnitIndex { get; set; }
        public string DeliveryLocation { get; set; }
        public DateTime RequiredDate { get; set; }
        public int Status { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
