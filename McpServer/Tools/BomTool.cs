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
        
        var partStrings = parts.Select(part => part.ToString());
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
        
        return part.ToString();
    }

    [UsedImplicitly]
    [McpServerTool]
    [Description("Shows the BOM for a part with the given id from the BOM API")]
    public async ValueTask<string> ShowBom(
        [Description("The id of the BOM root part")] int id,
        CancellationToken cancellationToken)
    {
        var client = this.CreateClient();
        var requestUri = GetRequestUri(id, "showbom");
        var bomString = await client.ReadAsync(requestUri, cancellationToken);
        return bomString;
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

        var content = Serialize(partDto);
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

        var content = Serialize(partDto);
        var client = this.CreateClient();
        var requestUri = GetRequestUri(id);
        var response = await client.PatchAsync(requestUri, content, cancellationToken);

        return response.IsSuccessStatusCode 
            ? "Part updated successfully." 
            : $"Failed to update part: {response.ReasonPhrase}";
    }

    [UsedImplicitly]
    [McpServerTool]
    [Description("Adds a new part with the given name and number below the parent part with the given id using the BOM API")]
    public async ValueTask<string> AddSubPart(
        [Description("The id of the parent part")] int id, 
        [Description("The name of the sub part")] string name, 
        [Description("The number of the sub part")] string number,
        CancellationToken cancellationToken)
    {
        var partDto = new PartDto
        {
            Name = name,
            Number = number
        };

        var content = Serialize(partDto);
        var client = this.CreateClient();
        var requestUri = GetRequestUri(id, "addsubpart");
        var response = await client.PatchAsync(requestUri, content, cancellationToken);

        return response.IsSuccessStatusCode 
            ? "Sub part updated successfully." 
            : $"Failed to create sub part: {response.ReasonPhrase}";
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

    private static Uri GetRequestUri(int? id = null, string subpath = "")
    {
        return id.HasValue 
            ? new Uri($"bom/{subpath + "/"}{id}", UriKind.Relative) 
            : new Uri($"bom/{subpath}", UriKind.Relative);
    }

    private static StringContent Serialize<T>(T obj)
    {
        var partString = JsonSerializer.Serialize(obj);
        return new StringContent(partString, System.Text.Encoding.UTF8, "application/json");
    }
    
    private HttpClient CreateClient()
    {
        return httpClientFactory.CreateClient("BomApiClient");
    }
    
    #endregion
}