using System.Text;
using BomDataAccess;
using Microsoft.AspNetCore.Mvc;

namespace BomAPI.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class BomController(ILogger<BomController> logger) : ControllerBase
{
    #region find
    
    [HttpGet("{id:int}")]
    public async ValueTask<PartDto?> Find(int id, CancellationToken cancellationToken)
    {
        var part = await BomRepository.Find(id, logger, cancellationToken);
        if (part is null) return null;

        return new PartDto
        {
            Id = part.Id,
            Name = part.Name,
            Number = part.Number
        };
    }
    
    [HttpGet]
    public async ValueTask<IEnumerable<PartDto>> FindAll(CancellationToken cancellationToken)
    {
        var parts = await BomRepository.FindAll(logger, cancellationToken);
        return parts.Select(part => new PartDto
        {
            Id = part.Id,
            Name = part.Name,
            Number = part.Number,
            ParentId = part.Parent?.Id
        });
    }

    [HttpGet("showbom/{id:int}")]
    public async ValueTask<string> ShowBom(int id, CancellationToken cancellationToken)
    {
        var rootPart = await BomRepository.LoadBom(id, logger, cancellationToken);
        if (rootPart is null) return string.Empty;
        
        var builder = new StringBuilder();
        PrintBomRecursive(rootPart, builder, 0);
        return builder.ToString();
    }
    
    private static void PrintBomRecursive(Part part, StringBuilder builder, int indentLevel)
    {
        var indent = new string(' ', indentLevel * 2);
        var prefix = indentLevel == 0 ? "" : indent + "├─";
        var message = $"{prefix}Part Id: {part.Id}, Name: {part.Name}, Number: {part.Number}";
        builder.AppendLine(message);

        var children = part.Children.ToList();
        foreach (var child in children)
        {
            PrintBomRecursive(child, builder, indentLevel + 1);
        }
    }
    
    #endregion
    
    #region create
    
    [HttpPost]
    public ValueTask<int> Create([FromBody] PartDto partDto, CancellationToken cancellationToken)
    {
        var part = new Part
        {
            Name = partDto.Name,
            Number = partDto.Number
        };

        return BomRepository.Create(part, logger, cancellationToken);
    }
    
    #endregion
    
    #region edit
    
    [HttpPatch("{id:int}")]
    public ValueTask Edit(int id, [FromBody] PartDto partDto, CancellationToken cancellationToken)
    {
        var part = new Part
        {
            Id = id,
            Name = partDto.Name,
            Number = partDto.Number
        };

        return BomRepository.Update(part, logger, cancellationToken);
    }
    
    [HttpPatch("addsubpart/{id:int}")]
    public async ValueTask AddSubPart(int id, [FromBody] PartDto subPartDto, CancellationToken cancellationToken)
    {
        var subPart = new Part
        {
            Name = subPartDto.Name,
            Number = subPartDto.Number
        };

        await BomRepository.AddSubPart(id, subPart, logger, cancellationToken);
    }
    
    #endregion
    
    #region delete
    
    [HttpDelete("{id:int}")]
    public async ValueTask<bool> Delete(int id, CancellationToken cancellationToken)
    {
        return await BomRepository.Delete(id, logger, cancellationToken);
    }
    
    #endregion
}