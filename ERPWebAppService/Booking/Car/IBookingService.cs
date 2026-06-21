using ERPWebAppModels.Booking;

namespace ERPWebAppService.Booking.Car;

public interface IBookingService
{
    Task<IEnumerable<BookingDto>> GetAllAsync();
    Task<BookingOptionsDto> GetOptionsAsync();
    Task<BookingDto?> GetByIdAsync(int id);
    Task<BookingDto> CreateBookingAsync(CreateBookingDto dto);
    Task<BookingDto?> UpdateBookingAsync(int id, UpdateBookingDto dto);
    Task<bool> DeleteBookingAsync(int id);
}
