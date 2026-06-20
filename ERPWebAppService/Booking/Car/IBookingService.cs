using ERPWebAppModels.Booking;

namespace ERPWebAppService.Booking.Car
{
    public interface IBookingService
    {
        Task<BookingDto> CreateBookingAsync(CreateBookingDto dto);
        Task<IEnumerable<BookingDto>> GetAllAsync();
    }
}