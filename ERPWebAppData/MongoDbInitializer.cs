using MongoDB.Driver;
using WebApp.Data.Entity;

namespace WebApp.Data;

public sealed class MongoDbInitializer
{
    private readonly MongoDbContext _context;

    public MongoDbInitializer(MongoDbContext context) => _context = context;

    public async Task CreateIndexesAsync(CancellationToken cancellationToken = default)
    {
        await _context.Users.Indexes.CreateManyAsync(
        [
            new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(x => x.NormalizedEmail),
                new CreateIndexOptions { Unique = true, Name = "UX_Users_Email" }),
            new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(x => x.NormalizedUserName),
                new CreateIndexOptions { Unique = true, Name = "UX_Users_UserName" })
        ], cancellationToken);

        await _context.Loans.Indexes.CreateManyAsync(
        [
            new CreateIndexModel<Loan>(
                Builders<Loan>.IndexKeys.Ascending(x => x.LoanNumber),
                new CreateIndexOptions { Unique = true, Name = "UX_Loans_LoanNumber" }),
            new CreateIndexModel<Loan>(
                Builders<Loan>.IndexKeys.Ascending(x => x.UserId),
                new CreateIndexOptions { Name = "IX_Loans_UserId" })
        ], cancellationToken);

        await _context.LoanPayments.Indexes.CreateOneAsync(
            new CreateIndexModel<LoanPayment>(
                Builders<LoanPayment>.IndexKeys.Ascending(x => x.LoanId),
                new CreateIndexOptions { Name = "IX_LoanPayments_LoanId" }),
            cancellationToken: cancellationToken);

        await _context.LoanEMISchedules.Indexes.CreateOneAsync(
            new CreateIndexModel<LoanEMISchedule>(
                Builders<LoanEMISchedule>.IndexKeys.Ascending(x => x.LoanId),
                new CreateIndexOptions { Name = "IX_LoanEMISchedules_LoanId" }),
            cancellationToken: cancellationToken);
    }
}
