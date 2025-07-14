using System.ComponentModel;
using System.Text.Json;
using JetBrains.Annotations;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[UsedImplicitly]
[McpServerToolType]
[Description("BOM Tool for reading, creating and deleting of parts from the BOM API")]
public sealed class BomTool(IHttpClientFactory httpClientFactory)
{
    #region fields
    
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    #endregion
    
    #region loading
    
    [UsedImplicitly]
    [McpServerTool]
    [Description("Reads all available parts from the BOM API")]
    public async ValueTask<string> GetParts(
        CancellationToken cancellationToken)
    {
        var client = this.CreateClient();
        var requestUri = GetRequestUri();
        var result = await client.ReadJsonDocumentAsync(requestUri, cancellationToken);
        
        var parts = result.Deserialize<IEnumerable<PartDto>>(SerializerOptions);
        if (parts is null) return "No parts found.";
        
        var partStrings = parts.Select(part => $"Part with Id '{part.Id}', Name: '{part.Name}', Number: '{part.Number}'");
        return string.Join(Environment.NewLine, partStrings);
    }

    [UsedImplicitly]
    [McpServerTool]
    [Description("Reads a single part with the given id from the BOM API")]
    public async ValueTask<string> GetPart(
        [Description("The id of the part to load")] int id,
        CancellationToken cancellationToken)
    {
        var client = this.CreateClient();
        var requestUri = GetRequestUri(id);
        var result = await client.ReadJsonDocumentAsync(requestUri, cancellationToken);
        
        var part = result.Deserialize<PartDto>(SerializerOptions);
        if (part is null) return $"Part with Id '{id}' not found.";
        
        var partString = $"Part with Id '{part.Id}', Name: '{part.Name}', Number: '{part.Number}'";
        return partString;
    }

    #endregion
    
    #region creation
    
    [UsedImplicitly]
    [McpServerTool]
    [Description("Creates a new part with the given name and number using the BOM API")]
    public async ValueTask<string> CreatePart(
        [Description("The name of the part")] string name, 
        [Description("The number of the part")] string number,
        CancellationToken cancellationToken)
    {
        var partDto = new PartDto
        {
            Name = name,
            Number = number
        };

        var partString = JsonSerializer.Serialize(partDto, SerializerOptions);
        var content = new StringContent(partString, System.Text.Encoding.UTF8, "application/json");
        
        var client = this.CreateClient();
        var requestUri = GetRequestUri();
        var response = await client.PostAsync(requestUri, content, cancellationToken);

        return response.IsSuccessStatusCode 
            ? "Part created successfully." 
            : $"Failed to create part: {response.ReasonPhrase}";
    }

    #endregion
    
    #region updating
    
    [UsedImplicitly]
    [McpServerTool]
    [Description("Updates a new part with the given name and number using the BOM API")]
    public async ValueTask<string> UpdatePart(
        [Description("The id of the update part")] int id, 
        [Description("The name of the part")] string name, 
        [Description("The number of the part")] string number,
        CancellationToken cancellationToken)
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
        var response = await client.PatchAsync(requestUri, content, cancellationToken);

        return response.IsSuccessStatusCode 
            ? "Part updated successfully." 
            : $"Failed to update part: {response.ReasonPhrase}";
    }

    #endregion
    
    #region deletion
    
    [UsedImplicitly]
    [McpServerTool]
    [Description("Deletes a part with the given id using the BOM API")]
    public async ValueTask<string> DeletePart(
        [Description("The id of the part to delete")] int id,
        CancellationToken cancellationToken)
    {
        var client = this.CreateClient();
        var requestUri = GetRequestUri(id);
        var response = await client.DeleteAsync(requestUri, cancellationToken);

        return response.IsSuccessStatusCode 
            ? "Part deleted successfully." 
            : $"Failed to delete part: {response.ReasonPhrase}";
    }
    
    #endregion
    
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