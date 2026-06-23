using MongoDB.Driver;
using WebApp.Data;
using WebApp.Data.Entity;
using WebApp.Model.Transaction;

namespace WebApp.Service.Transaction;

public class LoanPaymentService : ILoanPaymentService
{
    private readonly MongoDbContext _context;
    private readonly IMongoSequenceService _sequences;

    public LoanPaymentService(MongoDbContext context, IMongoSequenceService sequences)
    {
        _context = context;
        _sequences = sequences;
    }

    public async Task<List<LoanPaymentDto>> GetAllAsync() =>
        (await _context.LoanPayments.Find(x => !x.IsDeleted)
            .SortByDescending(x => x.PaymentDate).ToListAsync()).Select(ToDto).ToList();

    public async Task<LoanPaymentDto?> GetByIdAsync(int id)
    {
        var item = await _context.LoanPayments
            .Find(x => x.Id == id && !x.IsDeleted).FirstOrDefaultAsync();
        return item == null ? null : ToDto(item);
    }

    public async Task<LoanPaymentDto> AddAsync(LoanPaymentDto model)
    {
        var loan = await _context.Loans
            .Find(x => x.Id == model.LoanId && !x.IsDeleted).FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Loan does not exist.");
        if (!loan.Active || loan.Status != "Active")
            throw new InvalidOperationException("Payments can only be recorded for approved active loans.");

        var schedule = await _context.LoanEMISchedules.Find(x =>
            x.Id == model.ScheduleId && x.LoanId == model.LoanId && !x.IsDeleted)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("The selected installment does not belong to this loan.");
        if (schedule.IsPaid)
            throw new InvalidOperationException("The selected installment is already paid.");

        var entity = new LoanPayment
        {
            Id = await _sequences.GetNextAsync("LoanPayments"),
            LoanId = model.LoanId, ScheduleId = model.ScheduleId,
            AmountPaid = model.AmountPaid, PaymentDate = model.PaymentDate,
            TransactionId = model.TransactionId, PaymentMode = model.PaymentMode,
            PaymentStatus = model.PaymentStatus, Remarks = model.Remarks,
            Active = true, F_Created_Date_Time = DateTime.UtcNow
        };
        await _context.LoanPayments.InsertOneAsync(entity);
        if (model.PaymentStatus.Equals("Success", StringComparison.OrdinalIgnoreCase)
            || model.PaymentStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase))
        {
            await _context.LoanEMISchedules.UpdateOneAsync(
                x => x.Id == schedule.Id,
                Builders<LoanEMISchedule>.Update
                    .Set(x => x.IsPaid, true)
                    .Set(x => x.PaidDate, model.PaymentDate)
                    .Set(x => x.F_Updated_Date_Time, DateTime.UtcNow));
        }
        return ToDto(entity);
    }

    public async Task<bool> UpdateAsync(int id, LoanPaymentDto model)
    {
        var result = await _context.LoanPayments.UpdateOneAsync(
            x => x.Id == id && !x.IsDeleted,
            Builders<LoanPayment>.Update
                .Set(x => x.LoanId, model.LoanId)
                .Set(x => x.PaymentMode, model.PaymentMode)
                .Set(x => x.Remarks, model.Remarks)
                .Set(x => x.F_Updated_Date_Time, DateTime.UtcNow));
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var result = await _context.LoanPayments.UpdateOneAsync(
            x => x.Id == id && !x.IsDeleted,
            Builders<LoanPayment>.Update
                .Set(x => x.IsDeleted, true)
                .Set(x => x.Active, false)
                .Set(x => x.F_Updated_Date_Time, DateTime.UtcNow));
        return result.ModifiedCount > 0;
    }

    private static LoanPaymentDto ToDto(LoanPayment x) => new()
    {
        Id = x.Id, LoanId = x.LoanId, ScheduleId = x.ScheduleId,
        AmountPaid = x.AmountPaid, PaymentDate = x.PaymentDate,
        TransactionId = x.TransactionId, PaymentMode = x.PaymentMode,
        PaymentStatus = x.PaymentStatus, Remarks = x.Remarks
    };
}
