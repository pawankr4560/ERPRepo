using MongoDB.Driver;
using WebApp.Data;
using WebApp.Data.Entity;
using WebApp.Model.Transaction;

namespace WebApp.Service.Transaction;

public class LoanEMIScheduleService : ILoanEMIScheduleService
{
    private readonly MongoDbContext _context;
    private readonly IMongoSequenceService _sequences;

    public LoanEMIScheduleService(MongoDbContext context, IMongoSequenceService sequences)
    {
        _context = context;
        _sequences = sequences;
    }

    public async Task<List<LoanEMIScheduleDto>> GetAllAsync() =>
        (await _context.LoanEMISchedules.Find(x => !x.IsDeleted)
            .SortBy(x => x.InstallmentNo).ToListAsync()).Select(ToDto).ToList();

    public async Task<LoanEMIScheduleDto?> GetByIdAsync(int id)
    {
        var item = await _context.LoanEMISchedules
            .Find(x => x.Id == id && !x.IsDeleted).FirstOrDefaultAsync();
        return item == null ? null : ToDto(item);
    }

    public async Task<List<LoanEMIScheduleDto>> GetScheduleByLoanNumberAsync(string loanNumber)
    {
        var loan = await _context.Loans.Find(x =>
            x.LoanNumber == loanNumber && !x.IsDeleted && x.Active && x.Status == "Active")
            .FirstOrDefaultAsync();
        if (loan == null) return [];
        var detail = await _context.LoanCustomerDetails
            .Find(x => x.LoanId == loan.Id && !x.IsDeleted).FirstOrDefaultAsync();
        var result = (await _context.LoanEMISchedules
            .Find(x => x.LoanId == loan.Id && !x.IsDeleted)
            .SortBy(x => x.InstallmentNo).ToListAsync()).Select(ToDto).ToList();
        foreach (var dto in result)
        {
            dto.CustMobNo = detail?.CustomerMobileNo;
            dto.GranterMobNo = detail?.GuarantorMobileNo;
            dto.GranterName = detail?.GuarantorName;
            dto.Relation = detail?.GuarantorRelationship;
        }
        return result;
    }

    public async Task<List<LoanEMIScheduleDto>> GetByLoanIdAsync(int loanId) =>
        (await _context.LoanEMISchedules
            .Find(x => x.LoanId == loanId && !x.IsDeleted)
            .SortBy(x => x.InstallmentNo).ToListAsync()).Select(ToDto).ToList();

    public async Task<LoanEMIScheduleDto> AddAsync(LoanEMIScheduleDto model)
    {
        var entity = FromDto(model);
        entity.Id = await _sequences.GetNextAsync("LoanEMISchedules");
        entity.Active = true;
        entity.IsDeleted = false;
        entity.F_Created_Date_Time = DateTime.UtcNow;
        await _context.LoanEMISchedules.InsertOneAsync(entity);
        return ToDto(entity);
    }

    public async Task<List<LoanInstallmentDto>> GetUnpaidInstallmentsByLoanNumber(string loanNumber)
    {
        var loan = await _context.Loans
            .Find(x => x.LoanNumber == loanNumber && !x.IsDeleted)
            .FirstOrDefaultAsync();
        if (loan == null) return [];
        return (await _context.LoanEMISchedules.Find(x =>
                x.LoanId == loan.Id && !x.IsDeleted && !x.IsPaid)
            .SortBy(x => x.InstallmentNo).ToListAsync())
            .Select(x => new LoanInstallmentDto
            {
                LoanId = loan.Id, ScheduleId = x.Id, InstallmentNo = x.InstallmentNo,
                DueDate = x.DueDate, EMIAmount = x.EMIAmount,
                PrincipalAmount = x.PrincipalAmount, InterestAmount = x.InterestAmount,
                OutstandingBalance = x.OutstandingBalance
            }).ToList();
    }

    public async Task<bool> UpdateAsync(int id, LoanEMIScheduleDto model)
    {
        var entity = await _context.LoanEMISchedules
            .Find(x => x.Id == id && !x.IsDeleted).FirstOrDefaultAsync();
        if (entity == null) return false;
        entity.LoanId = model.LoanId;
        entity.InstallmentNo = model.InstallmentNo;
        entity.DueDate = model.DueDate;
        entity.EMIAmount = model.EMIAmount;
        entity.PrincipalAmount = model.PrincipalAmount;
        entity.InterestAmount = model.InterestAmount;
        entity.OutstandingBalance = model.OutstandingBalance;
        entity.IsPaid = model.IsPaid;
        entity.PaidDate = model.PaidDate;
        entity.F_Updated_Date_Time = DateTime.UtcNow;
        var result = await _context.LoanEMISchedules.ReplaceOneAsync(x => x.Id == id, entity);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var result = await _context.LoanEMISchedules.UpdateOneAsync(
            x => x.Id == id && !x.IsDeleted,
            Builders<LoanEMISchedule>.Update
                .Set(x => x.IsDeleted, true)
                .Set(x => x.Active, false)
                .Set(x => x.F_Updated_Date_Time, DateTime.UtcNow));
        return result.ModifiedCount > 0;
    }

    private static LoanEMIScheduleDto ToDto(LoanEMISchedule x) => new()
    {
        Id = x.Id, LoanId = x.LoanId, InstallmentNo = x.InstallmentNo,
        DueDate = x.DueDate, EMIAmount = x.EMIAmount, PrincipalAmount = x.PrincipalAmount,
        InterestAmount = x.InterestAmount, OutstandingBalance = x.OutstandingBalance,
        IsPaid = x.IsPaid, PaidDate = x.PaidDate
    };
    private static LoanEMISchedule FromDto(LoanEMIScheduleDto x) => new()
    {
        LoanId = x.LoanId, InstallmentNo = x.InstallmentNo, DueDate = x.DueDate,
        EMIAmount = x.EMIAmount, PrincipalAmount = x.PrincipalAmount,
        InterestAmount = x.InterestAmount, OutstandingBalance = x.OutstandingBalance,
        IsPaid = x.IsPaid, PaidDate = x.PaidDate
    };
}
