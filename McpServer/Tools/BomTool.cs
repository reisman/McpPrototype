using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[UsedImplicitly]
[McpServerToolType]
[Description("BOM Tool for reading, creating and deleting of parts from the BOM API")]
public sealed class BomTool(IHttpClientFactory httpClientFactory)
{
    #region loading
    
    [UsedImplicitly]
    [McpServerTool]
    [Description("Reads all available parts from the BOM API")]
    public async ValueTask<string> GetParts(CancellationToken cancellationToken)
    {
        var requestUri = GetRequestUri();
        
        var parts = await this.CreateClient().Get<IEnumerable<PartDto>>(requestUri, cancellationToken);
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
        var requestUri = GetRequestUri(id);
        var part = await this.CreateClient().Get<PartDto>(requestUri, cancellationToken);
        return part is null 
            ? $"Part with Id '{id}' not found." 
            : part.ToString();
    }

    [UsedImplicitly]
    [McpServerTool]
    [Description("Shows the BOM for a part with the given id from the BOM API")]
    public async ValueTask<string> ShowBom(
        [Description("The id of the BOM root part")] int id,
        CancellationToken cancellationToken)
    {
        var requestUri = GetRequestUri(id, "showbom");
        return await this.CreateClient().GetString(requestUri, cancellationToken);
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
        
        var requestUri = GetRequestUri();
        await this.CreateClient().Post(requestUri, partDto, cancellationToken);
        return "Part created successfully.";
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
        
        var requestUri = GetRequestUri(id);
        await this.CreateClient().Patch(requestUri, partDto, cancellationToken);
        return "Part updated successfully.";
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
        
        var requestUri = GetRequestUri(id, "addsubpart");
        await this.CreateClient().Patch(requestUri, partDto, cancellationToken);
        return "Sub part updated successfully.";
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
        var requestUri = GetRequestUri(id);
        await this.CreateClient().Delete(requestUri, cancellationToken);
        return "Part deleted successfully.";
    }
    
    #endregion
    
    #region helper methods

    private static Uri GetRequestUri(int? id = null, string subpath = "")
    {
        return id.HasValue 
            ? new Uri($"bom/{subpath + "/"}{id}", UriKind.Relative) 
            : new Uri($"bom/{subpath}", UriKind.Relative);
    }
    
    private HttpClient CreateClient()
    {
        return httpClientFactory.CreateClient("BomApiClient");
    }
    
    #endregion
}