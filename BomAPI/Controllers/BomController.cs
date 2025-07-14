using BomDataAccess;
using Microsoft.AspNetCore.Mvc;

namespace BomAPI.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class BomController : ControllerBase
{
    #region find
    
    [HttpGet("{id:int}")]
    public async ValueTask<PartDto?> Find(int id, CancellationToken cancellationToken)
    {
        var part = await BomRepository.Find(id, cancellationToken);
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
        var parts = await BomRepository.FindAll(cancellationToken);
        return parts.Select(part => new PartDto
        {
            Id = part.Id,
            Name = part.Name,
            Number = part.Number
        });
    }
    
    #endregion
    
    #region create
    
    [HttpPost(Name = "Create")]
    public ValueTask<int> Create([FromBody] PartDto partDto, CancellationToken cancellationToken)
    {
        var part = new Part
        {
            Name = partDto.Name,
            Number = partDto.Number
        };

        return BomRepository.Create(part, cancellationToken);
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

        return BomRepository.Update(part, cancellationToken);
    }
    
    #endregion
    
    #region delete
    
    [HttpDelete("{id:int}")]
    public async ValueTask<bool> Delete(int id, CancellationToken cancellationToken)
    {
        return await BomRepository.Delete(id, cancellationToken);
    }
    
    #endregion
}