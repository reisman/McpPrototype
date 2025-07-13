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
        var result = await client.ReadJsonDocumentAsync("bom");
        
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
        var result = await client.ReadJsonDocumentAsync($"bom/{id}");
        
        var part = result.Deserialize<PartDto>(SerializerOptions);
        if (part is null) return $"Part with Id '{id}' not found.";
        
        var partString = $"Part with Id '{part.Id}', Name: '{part.Name}', Number: '{part.Number}'";
        return partString;
    }
    
    [UsedImplicitly]
    [McpServerTool, Description("Creates a new part with the given name and number using the BOM API")]
    public async ValueTask<string> CreatePart(string name, string number)
    {
        var client = this.CreateClient();
        var partDto = new PartDto
        {
            Name = name,
            Number = number
        };

        var partString = JsonSerializer.Serialize(partDto);
        var content = new StringContent(partString, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("bom", content);

        return response.IsSuccessStatusCode 
            ? "Part created successfully." 
            : $"Failed to create part: {response.ReasonPhrase}";
    }
   
    private HttpClient CreateClient()
    {
        return httpClientFactory.CreateClient("BomApiClient");
    }
}