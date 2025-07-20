namespace BomAPI.Controllers;

public class PartDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Number { get; init; } = string.Empty;
    public int? ParentId { get; init; }
    
    public override string ToString()
    {
        return $"Part with Id '{this.Id}', Name: '{this.Name}', Number: '{this.Number}', Parent: '{this.ParentId}'";
    }
}