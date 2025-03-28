namespace Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public string CategoryId { get; set; }
    
    public decimal? Price { get; set; }
}