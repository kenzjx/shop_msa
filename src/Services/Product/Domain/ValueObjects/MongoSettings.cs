namespace Domain.ValueObjects;

public class MongoSettings
{
    public const string SectionName = "MongoDB";
    /// <summary>
    /// connect db
    /// </summary>
    public string ConnectionString { get; set; }
    
    public string DatabaseName { get; set; }
}