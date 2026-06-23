using MongoDB.Driver;
using WebApp.Data;
using WebApp.Data.Entity;
using WebApp.Model.Transaction;
using WebApp.Service.Message;

namespace WebApp.Service.Transaction;

public class LoanService : ILoanService
{
    private const string PendingStatus = "Pending";
    private const string ActiveStatus = "Active";
    private const string RejectedStatus = "Rejected";
    private readonly MongoDbContext _context;
    private readonly IMongoSequenceService _sequences;
    private readonly IMessageService _messageService;

    public LoanService(
        MongoDbContext context,
        IMongoSequenceService sequences,
        IMessageService messageService)
    {
        _context = context;
        _sequences = sequences;
        _messageService = messageService;
    }

    public async Task<List<LoanDto>> LoanList()
    {
        var loans = await _context.Loans.Find(x => !x.IsDeleted).ToListAsync();
        var userIds = loans.Select(x => x.UserId).Distinct().ToList();
        var users = await _context.Users
            .Find(Builders<User>.Filter.In(x => x.Id, userIds))
            .ToListAsync();
        var names = users.ToDictionary(x => x.Id, FullName);
        return loans.Select(x => ToDto(
            x,
            names.GetValueOrDefault(x.UserId, string.Empty))).ToList();
    }

    public async Task<LoanDto?> GetLoanById(int id)
    {
        var loan = await _context.Loans
            .Find(x => x.Id == id && !x.IsDeleted)
            .FirstOrDefaultAsync();
        if (loan == null) return null;

        var user = await _context.Users.Find(x => x.Id == loan.UserId).FirstOrDefaultAsync();
        var dto = ToDto(loan, user == null ? string.Empty : FullName(user));
        var detail = await _context.LoanCustomerDetails
            .Find(x => x.LoanId == id && !x.IsDeleted)
            .FirstOrDefaultAsync();
        if (detail != null)
        {
            dto.CustomerDetail = new LoanCustomerDetailRequestModel
            {
                CustomerAadhaarNo = detail.CustomerAadhaarNo,
                CustomerMobileNo = detail.CustomerMobileNo,
                CustomerAddress = detail.CustomerAddress,
                CustomerCity = detail.CustomerCity,
                CustomerState = detail.CustomerState,
                CustomerPinCode = detail.CustomerPinCode,
                GuarantorName = detail.GuarantorName,
                GuarantorAadhaarNo = detail.GuarantorAadhaarNo,
                GuarantorMobileNo = detail.GuarantorMobileNo,
                GuarantorAddress = detail.GuarantorAddress,
                GuarantorRelationship = detail.GuarantorRelationship
            };
        }

        dto.EmiSchedules = (await _context.LoanEMISchedules
            .Find(x => x.LoanId == id && !x.IsDeleted)
            .SortBy(x => x.InstallmentNo)
            .ToListAsync()).Select(ToScheduleDto).ToList();
        dto.Payments = (await _context.LoanPayments
            .Find(x => x.LoanId == id && !x.IsDeleted)
            .SortByDescending(x => x.PaymentDate)
            .ToListAsync()).Select(ToPaymentDto).ToList();
        return dto;
    }

    public async Task<bool> AddLoan(LoanRequestModel model)
    {
        if (await _context.Loans.Find(x => x.LoanNumber == model.LoanNumber).AnyAsync())
            throw new Exception("This Loan number already exists.");

        var now = DateTime.UtcNow;
        var loan = new Loan
        {
            Id = await _sequences.GetNextAsync("Loans"),
            UserId = model.UserId,
            LoanNumber = model.LoanNumber,
            LoanAmount = model.LoanAmount,
            Rate = model.Rate,
            Tenure = model.Tenure,
            EMI = model.EMI,
            IsReducingInterest = model.interestCalculationType,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            Status = PendingStatus,
            Active = false,
            IsDeleted = false,
            F_Created_Date_Time = now,
            F_Updated_Date_Time = now
        };
        await _context.Loans.InsertOneAsync(loan);

        var customer = model.CustomerDetail;
        await _context.LoanCustomerDetails.InsertOneAsync(new LoanCustomerDetail
        {
            Id = await _sequences.GetNextAsync("LoanCustomerDetails"),
            LoanId = loan.Id,
            CustomerAadhaarNo = customer.CustomerAadhaarNo,
            CustomerMobileNo = customer.CustomerMobileNo,
            CustomerAddress = customer.CustomerAddress,
            CustomerCity = customer.CustomerCity,
            CustomerState = customer.CustomerState,
            CustomerPinCode = customer.CustomerPinCode,
            GuarantorName = customer.GuarantorName,
            GuarantorAadhaarNo = customer.GuarantorAadhaarNo,
            GuarantorMobileNo = customer.GuarantorMobileNo,
            GuarantorAddress = customer.GuarantorAddress,
            GuarantorRelationship = customer.GuarantorRelationship,
            F_Created_Date_Time = now
        });
        return true;
    }

    public async Task<CreateLoanDTO> GetLoanNumber()
    {
        var prefix = $"{DateTime.UtcNow.Year}-GKFIN-";
        var last = await _context.Loans
            .Find(x => x.LoanNumber.StartsWith(prefix))
            .SortByDescending(x => x.Id)
            .Limit(1)
            .FirstOrDefaultAsync();
        var next = 1;
        if (last != null && int.TryParse(last.LoanNumber.Split('-').Last(), out var sequence))
            next = sequence + 1;

        var users = await _context.Users
            .Find(x => !x.IsDeleted && x.Roles.Contains("User"))
            .SortBy(x => x.FirstName)
            .ToListAsync();
        return new CreateLoanDTO
        {
            LoanNumber = $"{prefix}{next:D5}",
            CustomerList = users.Select(x => new CustomerDropdownDto
            {
                Id = x.Id,
                CustomerName = FullName(x)
            }).ToList()
        };
    }

    public async Task<bool> UpdateLoan(LoanUpdateRequestModel model)
    {
        var loan = await _context.Loans
            .Find(x => x.Id == model.Id && !x.IsDeleted)
            .FirstOrDefaultAsync();
        if (loan == null) return false;
        if (loan.Status == ActiveStatus)
            throw new InvalidOperationException("Approved loans cannot be updated.");
        if (await _context.LoanEMISchedules
            .Find(x => x.LoanId == loan.Id && x.IsPaid && !x.IsDeleted).AnyAsync())
            throw new Exception("Loan cannot be updated because EMI payment already exists.");

        loan.UserId = model.UserId;
        loan.LoanNumber = model.LoanNumber;
        loan.LoanAmount = model.LoanAmount;
        loan.Rate = model.Rate;
        loan.EMI = model.EMI;
        loan.IsReducingInterest = model.InterestCalculationType;
        loan.Tenure = model.Tenure;
        loan.StartDate = model.StartDate;
        loan.EndDate = model.EndDate;
        loan.F_Updated_Date_Time = DateTime.UtcNow;
        await _context.Loans.ReplaceOneAsync(x => x.Id == loan.Id, loan);
        await _context.LoanEMISchedules.UpdateManyAsync(
            x => x.LoanId == loan.Id && !x.IsDeleted,
            Builders<LoanEMISchedule>.Update
                .Set(x => x.IsDeleted, true)
                .Set(x => x.Active, false)
                .Set(x => x.F_Updated_Date_Time, DateTime.UtcNow));
        return true;
    }

    public async Task<bool> ApproveLoan(int id, string approvedByUserId)
    {
        var loan = await _context.Loans.Find(x => x.Id == id && !x.IsDeleted).FirstOrDefaultAsync();
        if (loan == null) return false;
        if (loan.Status != PendingStatus)
            throw new InvalidOperationException("Only pending loans can be approved.");
        if (await _context.LoanEMISchedules
            .Find(x => x.LoanId == id && x.IsPaid && !x.IsDeleted).AnyAsync())
            throw new InvalidOperationException("A loan with paid installments cannot be approved.");

        loan.Status = ActiveStatus;
        loan.Active = true;
        loan.ApprovedAtUtc = DateTime.UtcNow;
        loan.ApprovedByUserId = approvedByUserId;
        loan.RejectedAtUtc = null;
        loan.RejectedByUserId = null;
        loan.F_Updated_Date_Time = DateTime.UtcNow;
        await _context.Loans.ReplaceOneAsync(x => x.Id == id, loan);

        if (!await _context.LoanEMISchedules.Find(x => x.LoanId == id && !x.IsDeleted).AnyAsync())
            await GenerateScheduleAsync(loan);
        return true;
    }

    public async Task<bool> RejectLoan(int id, string rejectedByUserId)
    {
        var loan = await _context.Loans.Find(x => x.Id == id && !x.IsDeleted).FirstOrDefaultAsync();
        if (loan == null) return false;
        if (loan.Status != PendingStatus)
            throw new InvalidOperationException("Only pending loans can be rejected.");
        if (await _context.LoanEMISchedules
            .Find(x => x.LoanId == id && x.IsPaid && !x.IsDeleted).AnyAsync())
            throw new InvalidOperationException("A loan with paid installments cannot be rejected.");

        loan.Status = RejectedStatus;
        loan.Active = false;
        loan.RejectedAtUtc = DateTime.UtcNow;
        loan.RejectedByUserId = rejectedByUserId;
        loan.ApprovedAtUtc = null;
        loan.ApprovedByUserId = null;
        loan.F_Updated_Date_Time = DateTime.UtcNow;
        var result = await _context.Loans.ReplaceOneAsync(x => x.Id == id, loan);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteLoan(int id)
    {
        var loan = await _context.Loans.Find(x => x.Id == id && !x.IsDeleted).FirstOrDefaultAsync();
        if (loan == null) return false;
        if (loan.Status == ActiveStatus)
            throw new InvalidOperationException("Approved loans cannot be deleted.");
        var result = await _context.Loans.UpdateOneAsync(
            x => x.Id == id,
            Builders<Loan>.Update
                .Set(x => x.IsDeleted, true)
                .Set(x => x.Active, false)
                .Set(x => x.F_Updated_Date_Time, DateTime.UtcNow));
        return result.ModifiedCount > 0;
    }

    private async Task GenerateScheduleAsync(Loan loan)
    {
        var schedules = loan.IsReducingInterest
            ? BuildReducingSchedule(loan)
            : BuildFlatSchedule(loan);
        foreach (var schedule in schedules)
            schedule.Id = await _sequences.GetNextAsync("LoanEMISchedules");
        if (schedules.Count > 0)
            await _context.LoanEMISchedules.InsertManyAsync(schedules);
    }

    private static List<LoanEMISchedule> BuildFlatSchedule(Loan loan)
    {
        var result = new List<LoanEMISchedule>();
        var principal = Convert.ToDecimal(loan.LoanAmount);
        var totalInterest = principal * Convert.ToDecimal(loan.Rate) * loan.Tenure / 12 / 100;
        var monthlyInterest = totalInterest / loan.Tenure;
        var monthlyPrincipal = principal / loan.Tenure;
        var outstanding = principal;
        for (var installment = 1; installment <= loan.Tenure; installment++)
        {
            var principalAmount = installment == loan.Tenure ? outstanding : monthlyPrincipal;
            outstanding -= principalAmount;
            result.Add(NewSchedule(
                loan.Id, installment, loan.StartDate.AddMonths(installment),
                principalAmount + monthlyInterest, principalAmount, monthlyInterest, outstanding));
        }
        return result;
    }

    private static List<LoanEMISchedule> BuildReducingSchedule(Loan loan)
    {
        var result = new List<LoanEMISchedule>();
        var principal = Convert.ToDecimal(loan.LoanAmount);
        var monthlyRate = Convert.ToDecimal(loan.Rate) / 12 / 100;
        var outstanding = principal;
        var emi = monthlyRate == 0
            ? principal / loan.Tenure
            : principal * monthlyRate
              * (decimal)Math.Pow(Convert.ToDouble(1 + monthlyRate), loan.Tenure)
              / ((decimal)Math.Pow(Convert.ToDouble(1 + monthlyRate), loan.Tenure) - 1);
        for (var installment = 1; installment <= loan.Tenure; installment++)
        {
            var interest = outstanding * monthlyRate;
            var principalAmount = installment == loan.Tenure ? outstanding : emi - interest;
            var installmentEmi = principalAmount + interest;
            outstanding -= principalAmount;
            result.Add(NewSchedule(
                loan.Id, installment, loan.StartDate.AddMonths(installment - 1),
                installmentEmi, principalAmount, interest, outstanding));
        }
        return result;
    }

    private static LoanEMISchedule NewSchedule(
        int loanId, int installment, DateTime dueDate, decimal emi,
        decimal principal, decimal interest, decimal outstanding) => new()
    {
        LoanId = loanId,
        InstallmentNo = installment,
        DueDate = dueDate,
        EMIAmount = Math.Round(emi, 2),
        PrincipalAmount = Math.Round(principal, 2),
        InterestAmount = Math.Round(interest, 2),
        OutstandingBalance = Math.Round(Math.Max(0, outstanding), 2),
        Active = true,
        F_Created_Date_Time = DateTime.UtcNow,
        F_Updated_Date_Time = DateTime.UtcNow
    };

    private static string FullName(User user) => $"{user.FirstName} {user.LastName}".Trim();
    private static LoanDto ToDto(Loan x, string userName) => new()
    {
        Id = x.Id, UserId = x.UserId, UserName = userName, LoanNumber = x.LoanNumber,
        LoanAmount = x.LoanAmount, Rate = x.Rate, EMI = x.EMI,
        InterestCalculationType = x.IsReducingInterest, Tenure = x.Tenure,
        StartDate = x.StartDate, EndDate = x.EndDate, Status = x.Status, Active = x.Active,
        ApprovedAtUtc = x.ApprovedAtUtc, ApprovedByUserId = x.ApprovedByUserId,
        RejectedAtUtc = x.RejectedAtUtc, RejectedByUserId = x.RejectedByUserId,
        CreatedDateTime = x.F_Created_Date_Time, UpdatedDateTime = x.F_Updated_Date_Time,
        CreatedBy = x.F_User_Index_Created
    };
    private static LoanEMIScheduleDto ToScheduleDto(LoanEMISchedule x) => new()
    {
        Id = x.Id, LoanId = x.LoanId, InstallmentNo = x.InstallmentNo,
        DueDate = x.DueDate, EMIAmount = x.EMIAmount, PrincipalAmount = x.PrincipalAmount,
        InterestAmount = x.InterestAmount, OutstandingBalance = x.OutstandingBalance,
        IsPaid = x.IsPaid, PaidDate = x.PaidDate
    };
    private static LoanPaymentDto ToPaymentDto(LoanPayment x) => new()
    {
        Id = x.Id, LoanId = x.LoanId, ScheduleId = x.ScheduleId,
        AmountPaid = x.AmountPaid, PaymentDate = x.PaymentDate,
        TransactionId = x.TransactionId, PaymentMode = x.PaymentMode,
        PaymentStatus = x.PaymentStatus, Remarks = x.Remarks
    };
}
