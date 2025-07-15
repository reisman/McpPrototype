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
        var part = await Find(id, cancellationToken);
        if (part is null) return null;
        
        await using var context = BomDbContext.Create();
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