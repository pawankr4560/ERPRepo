namespace ERPWebAppData.Entity
{
    public class ConstructionOrder
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        public int? QuoteId { get; set; } // nullable, because direct order may not need quote
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public decimal Quantity { get; set; }
        public int UnitId { get; set; }
        public string DeliveryLocation { get; set; }
        public DateTime DeliveryDate { get; set; }

        public int Status { get; set; } // Placed, Confirmed, Packed, Shipped, Delivered, Cancelled
        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
