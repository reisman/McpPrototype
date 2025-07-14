using System.ComponentModel;
using System.Text.Json;
using JetBrains.Annotations;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[UsedImplicitly]
[McpServerToolType, Description("BOM Tool for reading, creating and deleting of parts from the BOM API")]
public sealed class BomTool(IHttpClientFactory httpClientFactory)
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    
    [UsedImplicitly]
    [McpServerTool, Description("Reads all available parts from the BOM API")]
    public async ValueTask<string> GetParts()
    {
        var client = this.CreateClient();
        var requestUri = GetRequestUri();
        var result = await client.ReadJsonDocumentAsync(requestUri);
        
        var parts = result.Deserialize<IEnumerable<PartDto>>(SerializerOptions);
        if (parts is null) return "No parts found.";
        
        var partStrings = parts.Select(part => $"Part with Id '{part.Id}', Name: '{part.Name}', Number: '{part.Number}'");
        return string.Join(Environment.NewLine, partStrings);
    }

    [UsedImplicitly]
    [McpServerTool, Description("Reads a single part with the given id from the BOM API")]
    public async ValueTask<string> GetPart(int id)
    {
        var client = this.CreateClient();
        var requestUri = GetRequestUri(id);
        var result = await client.ReadJsonDocumentAsync(requestUri);
        
        var part = result.Deserialize<PartDto>(SerializerOptions);
        if (part is null) return $"Part with Id '{id}' not found.";
        
        var partString = $"Part with Id '{part.Id}', Name: '{part.Name}', Number: '{part.Number}'";
        return partString;
    }
    
    [UsedImplicitly]
    [McpServerTool, Description("Creates a new part with the given name and number using the BOM API")]
    public async ValueTask<string> CreatePart(string name, string number)
    {
        var partDto = new PartDto
        {
            Name = name,
            Number = number
        };

        var partString = JsonSerializer.Serialize(partDto);
        var content = new StringContent(partString, System.Text.Encoding.UTF8, "application/json");
        
        var client = this.CreateClient();
        var requestUri = GetRequestUri();
        var response = await client.PostAsync(requestUri, content);

        return response.IsSuccessStatusCode 
            ? "Part created successfully." 
            : $"Failed to create part: {response.ReasonPhrase}";
    }
   
    [UsedImplicitly]
    [McpServerTool, Description("Updates a new part with the given name and number using the BOM API")]
    public async ValueTask<string> UpdatePart(int id, string name, string number)
    {
        var partDto = new PartDto
        {
            Id = id,
            Name = name,
            Number = number
        };

        var partString = JsonSerializer.Serialize(partDto);
        var content = new StringContent(partString, System.Text.Encoding.UTF8, "application/json");
        
        var client = this.CreateClient();
        var requestUri = GetRequestUri(id);
        var response = await client.PatchAsync(requestUri, content);

        return response.IsSuccessStatusCode 
            ? "Part updated successfully." 
            : $"Failed to update part: {response.ReasonPhrase}";
    }
    
    [UsedImplicitly]
    [McpServerTool, Description("Deletes a part with the given id using the BOM API")]
    public async ValueTask<string> DeletePart(int id)
    {
        var client = this.CreateClient();
        var requestUri = GetRequestUri(id);
        var response = await client.DeleteAsync(requestUri);

        return response.IsSuccessStatusCode 
            ? "Part deleted successfully." 
            : $"Failed to delete part: {response.ReasonPhrase}";
    }
    
    #region helper methods

    private static Uri GetRequestUri(int? id = null)
    {
        return id.HasValue 
            ? new Uri($"bom/{id}", UriKind.Relative) 
            : new Uri("bom", UriKind.Relative);
    }
    
    private HttpClient CreateClient()
    {
        return httpClientFactory.CreateClient("BomApiClient");
    }
    
    #endregion
}