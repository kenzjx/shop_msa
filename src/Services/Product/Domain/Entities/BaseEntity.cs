using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class BaseEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    
    public DateTime Created { get; set; }
    
    public DateTime Modified { get; set; }
    
    public string CreatedBy { get; set; }
    
    public string ModifiedBy { get; set; }
    
    public bool IsDeleted { get; set; }
    
    public bool IsActive { get; set; }
}