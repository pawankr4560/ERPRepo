using MongoDB.Bson.Serialization.Attributes;

namespace WebApp.Data.Entity;

public sealed class SequenceCounter
{
    [BsonId]
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
}
