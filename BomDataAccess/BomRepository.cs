using Microsoft.EntityFrameworkCore;

namespace BomDataAccess;

public static class BomRepository
{
    public static async ValueTask<IReadOnlyCollection<Part>> FindAll(CancellationToken cancellationToken)
    {
        await using var context = BomDbContext.Create();
        return await context.Parts.ToListAsync(cancellationToken);
    }
    
    public static async ValueTask<Part?> Find(int id, CancellationToken cancellationToken)
    {
        await using var context = BomDbContext.Create();
        return await context.Parts.FindAsync([id], cancellationToken: cancellationToken);
    }
    
    public static async Task<Part?> LoadBom(int id, CancellationToken cancellationToken)
    {
        await using var context = BomDbContext.Create();
        var rootPart = await Find(id, cancellationToken);
        if (rootPart is null) return null;
        
        var currentParts = new[] { rootPart };
        while (currentParts.Any())
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
    
    public static async ValueTask<int> Create(Part part, CancellationToken cancellationToken)
    {
        await using var context = BomDbContext.Create();
        context.Parts.Add(part);
        await context.SaveChangesAsync(cancellationToken);
        return part.Id;
    }
    
    public static async ValueTask Update(Part part, CancellationToken cancellationToken)
    {
        await using var context = BomDbContext.Create();
        context.Parts.Attach(part);
        context.Entry(part).State = EntityState.Modified;
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public static async ValueTask<int?> AddSubPart(int id, Part subPart, CancellationToken cancellationToken)
    {
        await using var context = BomDbContext.Create();
        
        var part = await context.Parts
            .Include(p => p.Children)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (part is null) return null;

        context.Parts.Add(subPart);
        part.Children.Add(subPart);
        await context.SaveChangesAsync(cancellationToken);

        return subPart.Id;
    }
    
    public static async ValueTask<bool> Delete(int id, CancellationToken cancellationToken)
    {
        await using var context = BomDbContext.Create();
        
        var part = await Find(id, cancellationToken);
        if (part is null) return false;
        
        context.Parts.Remove(part);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}