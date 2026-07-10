namespace ERPWebApp.Server.Enum
{
    public enum ConstructionQuoteStatus
    {
        Pending = 0,
        PriceShared = 1,
        Accepted = 2,
        Rejected = 3,
        Completed = 4,
        Cancelled = 5,
        OrderPlaced = 6
    }
    public enum ConstructionOrderStatus
    {
        Placed = 0,
        Confirmed = 1,
        Processing = 2,
        Shipped = 3,
        Delivered = 4,
        Cancelled = 5
    }
    public enum ConstructionDeliveryStatus
    {
        Preparing = 0,
        Dispatched = 1,
        OutForDelivery = 2,
        Delivered = 3,
        Delayed = 4,
        Cancelled = 5
    }
}
