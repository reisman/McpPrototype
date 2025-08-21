using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BomDataAccess;

/// <summary>
/// Provides static methods for accessing and manipulating Part entities in the BOM database.
/// </summary>
public static class BomRepository
{
    /// <summary>
    /// Retrieves all Part entities from the database.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    public static async ValueTask<IReadOnlyCollection<Part>> FindAll(ILogger logger, CancellationToken cancellationToken)
    {
        await using var context = BomDbContext.Create();
        return await context.Parts.ToListAsync(cancellationToken);
    }
    
    /// <summary>
    /// Finds a Part entity by its id.
    /// </summary>
    /// <param name="id">The unique identifier of the Part.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public static async ValueTask<Part?> Find(int id, ILogger logger, CancellationToken cancellationToken)
    {
        await using var context = BomDbContext.Create();
        return await context.Parts.FindAsync([id], cancellationToken: cancellationToken);
    }
    
    /// <summary>
    /// Finds Part entities by their ids.
    /// </summary>
    /// <param name="ids">A collection of Part ids to search for.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public static async ValueTask<IReadOnlyDictionary<int, Part?>> Find(IReadOnlyCollection<int> ids, ILogger logger, CancellationToken cancellationToken)
    {
        await using var context = BomDbContext.Create();
        var results = await context
            .Parts
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();

        var resultMap = results.ToDictionary(p => p.Id, Part? (p) => p);
        foreach (var id in ids.Where(id => !resultMap.ContainsKey(id)))
        {
            resultMap.Add(id, null);
        }

        return resultMap;
    }
    
    /// <summary>
    /// Loads the Bill of Materials (BOM) for a given Part id, including all child parts.
    /// </summary>
    /// <param name="id">The unique identifier of the root Part.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <returns>The root Part with its children loaded, or null if not found.</returns>
    public static async Task<Part?> LoadBom(int id, ILogger logger, CancellationToken cancellationToken)
    {
        await using var context = BomDbContext.Create();
        var rootPart = await Find(id, logger, cancellationToken);
        if (rootPart is null) return null;
        
        var currentParts = new[] { rootPart };
        while (currentParts.Length > 0)
        {
            foreach (var currentPart in currentParts)
            {
                await context
                    .Entry(currentPart)
                    .Collection(b => b.Children)
                    .LoadAsync(cancellationToken);
            }

            currentParts = currentParts
                .SelectMany(part => part.Children)
                .ToArray();
        }

        return rootPart;
    }
    
    /// <summary>
    /// Creates a new Part entity in the database.
    /// </summary>
    /// <param name="part">The Part entity to create.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <returns>The id of the newly created Part.</returns>
    public static async ValueTask<int> Create(Part part, ILogger logger, CancellationToken cancellationToken)
    {
        await using var context = BomDbContext.Create();
        await context.Parts.AddAsync(part, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return part.Id;
    }
    
    /// <summary>
    /// Updates an existing Part entity in the database.
    /// </summary>
    /// <param name="part">The Part entity to update.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public static async ValueTask Update(Part part, ILogger logger, CancellationToken cancellationToken)
    {
        await using var context = BomDbContext.Create();
        context.Parts.Attach(part);
        context.Entry(part).State = EntityState.Modified;
        await context.SaveChangesAsync(cancellationToken);
    }
    
    /// <summary>
    /// Adds a sub-part to the Part with the specified id. Returns the new sub-part's id, or null if parent not found.
    /// </summary>
    /// <param name="id">The id of the parent Part.</param>
    /// <param name="subPart">The sub-part entity to add.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <returns>The id of the newly added sub-part, or null if parent not found.</returns>
    public static async ValueTask<int?> AddSubPart(int id, Part subPart, ILogger logger, CancellationToken cancellationToken)
    {
        await using var context = BomDbContext.Create();
        
        var part = await context.Parts
            .Include(p => p.Children)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (part is null) return null;

        await context.Parts.AddAsync(subPart, cancellationToken);
        part.Children.Add(subPart);
        await context.SaveChangesAsync(cancellationToken);

        return subPart.Id;
    }

    /// <summary>
    /// Creates a copy of the given Part entity and saves it to the database.
    /// </summary>
    /// <param name="sourcePart">The source Part entity to copy.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <returns>The copied Part entity.</returns>
    public static async ValueTask<Part> Copy(Part sourcePart, ILogger logger, CancellationToken cancellationToken)
    {
        await using var context = BomDbContext.Create();
        
        var copiedPart = new Part
        {
            Name = sourcePart.Name,
            Number = sourcePart.Number,
            Parent = sourcePart.Parent
        };
    
        await context.Parts.AddAsync(copiedPart, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        
        return copiedPart;
    }
    
    /// <summary>
    /// Deletes the Part entity with the specified id from the database.
    /// </summary>
    /// <param name="id">The unique identifier of the Part to delete.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <returns>True if the Part was deleted, false if not found.</returns>
    public static async ValueTask<bool> Delete(int id, ILogger logger, CancellationToken cancellationToken)
    {
        await using var context = BomDbContext.Create();
        var part = await Find(id, logger, cancellationToken);
        if (part is null) return false;
        context.Parts.Remove(part);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Calculates the total number of child parts within the BOM for the specified root part id using a recursive CTE for performance.
    /// </summary>
    /// <param name="id">The unique identifier of the root Part.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <returns>The total number of child parts (excluding the root part itself).</returns>
    public static async Task<int> CountChildPartsAsync(int id, ILogger logger, CancellationToken cancellationToken)
    {
        await using var context = BomDbContext.Create();
        var sql = @"
            WITH RecursiveParts AS (
                SELECT Id FROM Parts WHERE ParentId = {0}
                UNION ALL
                SELECT p.Id FROM Parts p
                INNER JOIN RecursiveParts rp ON p.ParentId = rp.Id
            )
            SELECT COUNT(*) AS Count FROM RecursiveParts
        ";

        return await context
            .Database
            .SqlQueryRaw<int>(sql, id)
            .SingleAsync(cancellationToken);
    }
}