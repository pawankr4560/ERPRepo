namespace ERPWebApp.Server.Enum
{
    public enum CarStatus
    {
        Available = 1,
        Booked = 2,
        Maintenance = 3,
        Inactive = 4
    }
    public enum BookingStatus
    {
        Pending = 1,
        Confirmed = 2,
        Active = 3,
        Completed = 4,
        Cancelled = 5
    }
    public enum PaymentStatus
    {
        Pending = 1,
        Paid = 2,
        Failed = 3,
        Refunded = 4
    }
}
