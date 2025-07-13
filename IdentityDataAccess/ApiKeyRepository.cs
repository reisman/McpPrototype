using Microsoft.EntityFrameworkCore;

namespace IdentityDataAccess;

public static class ApiKeyRepository
{
    public static async ValueTask<bool> IsValid(string key)
    {
        await using var context = ApiKeyContext.Create();
        return await context.ApiKeys.AnyAsync(k => k.Key == key);
    }

    public static async ValueTask<int> AddApiKey(string key)
    {
        var apiKey = new ApiKey { Key = key };
        await using var context = ApiKeyContext.Create();
        context.ApiKeys.Add(apiKey);
        await context.SaveChangesAsync();
        return apiKey.Id;
    }
}