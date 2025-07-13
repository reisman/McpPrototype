namespace BomDataAccess;

public sealed class Part
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required string Number { get; init; }
    public Part? Parent { get; init; }
    public List<Part> Children { get; } = [];
}