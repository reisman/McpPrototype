using Microsoft.EntityFrameworkCore;

namespace BomDataAccess;

public static class BomRepository
{
    public static async ValueTask<IReadOnlyCollection<Part>> FindAll()
    {
        await using var context = BomDbContext.Create();
        return await context.Parts.ToListAsync();
    }
    
    public static async ValueTask<Part?> Find(int id)
    {
        await using var context = BomDbContext.Create();
        return await context.Parts.FindAsync(id);
    }
    
    public static async ValueTask<int> Create(Part part)
    {
        await using var context = BomDbContext.Create();
        context.Parts.Add(part);
        await context.SaveChangesAsync();
        return part.Id;
    }
    
    public static async ValueTask Update(Part part)
    {
        await using var context = BomDbContext.Create();
        context.Parts.Attach(part);
        context.Entry(part).State = EntityState.Modified;
        await context.SaveChangesAsync();
    }
    
    public static async ValueTask<bool> Delete(int id)
    {
        await using var context = BomDbContext.Create();
        
        var part = await Find(id);
        if (part is null) return false;
        
        context.Parts.Remove(part);
        await context.SaveChangesAsync();
        return true;
    }
}