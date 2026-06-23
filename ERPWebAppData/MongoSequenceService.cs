using MongoDB.Driver;
using WebApp.Data.Entity;

namespace WebApp.Data;

public interface IMongoSequenceService
{
    Task<int> GetNextAsync(string sequenceName, CancellationToken cancellationToken = default);
}

public sealed class MongoSequenceService : IMongoSequenceService
{
    private readonly MongoDbContext _context;

    public MongoSequenceService(MongoDbContext context) => _context = context;

    public async Task<int> GetNextAsync(string sequenceName, CancellationToken cancellationToken = default)
    {
        var counter = await _context.Counters.FindOneAndUpdateAsync(
            x => x.Name == sequenceName,
            Builders<SequenceCounter>.Update.Inc(x => x.Value, 1),
            new FindOneAndUpdateOptions<SequenceCounter>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            },
            cancellationToken);

        return counter.Value;
    }
}
