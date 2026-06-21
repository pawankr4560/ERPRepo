using ERPWebAppModels.Booking;

namespace ERPWebAppService.Booking.Car;

public interface IBookingPaymentService
{
    Task<IEnumerable<BookingPaymentDto>> GetAllAsync(int? bookingId = null);
    Task<BookingPaymentDto?> GetByIdAsync(int id);
    Task<BookingPaymentDto> CreateAsync(BookingPaymentDto dto);
    Task<BookingPaymentDto?> UpdateAsync(int id, BookingPaymentDto dto);
    Task<bool> DeleteAsync(int id);
}
