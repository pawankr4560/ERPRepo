using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApp.Data.Entity;

public sealed class Role
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
}
