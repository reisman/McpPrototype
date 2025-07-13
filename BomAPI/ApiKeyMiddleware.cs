using System.Net;
using IdentityDataAccess;

namespace BomAPI;

public sealed class ApiKeyMiddleware(RequestDelegate next)
{
    private const string ApiKeyHeaderName = "X-API-KEY";

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            await AsNotAuthorized(context, "API Key is missing");
            return;
        }

        if (extractedApiKey.Count != 1)
        {
            await AsNotAuthorized(context,"Invalid API Key");
            return;
        }

        var extractedApiKeyValue = extractedApiKey.Single();
        if (string.IsNullOrEmpty(extractedApiKeyValue))
        {
            await AsNotAuthorized(context,"API Key is empty");
            return;
        }
        
        var isValidApiKey = await ApiKeyRepository.IsValid(extractedApiKeyValue);
        if (!isValidApiKey)
        {
            await AsNotAuthorized(context,"Invalid API Key");
            return;
        }
        
        await next(context);
    }

    private static async ValueTask AsNotAuthorized(HttpContext context, string message)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        await context.Response.WriteAsync(message);
    }
}