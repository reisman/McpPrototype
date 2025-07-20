using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[UsedImplicitly]
[McpServerToolType]
[Description("BOM Tool for reading, creating and deleting of parts from the BOM API")]
public sealed class BomTool(IHttpClientFactory httpClientFactory, ILogger<BomTool> logger)
{
    #region loading
    
    [UsedImplicitly]
    [McpServerTool]
    [Description("Reads all available parts from the BOM API")]
    public async ValueTask<string> GetParts(CancellationToken cancellationToken)
    {
        try
        {
            var requestUri = GetRequestUri();
    
            var parts = await this.CreateClient().Get<IEnumerable<PartDto>>(requestUri, cancellationToken);
            if (parts is null) return "No parts found.";
    
            var partStrings = parts.Select(part => part.ToString());
            return string.Join(Environment.NewLine, partStrings);
        }
        catch (Exception e)
        {
            logger.LogError("Error retrieving parts: {Message}", e.Message);
            return "Error retrieving parts.";
        }
    }
    
    [UsedImplicitly]
    [McpServerTool]
    [Description("Reads a single part with the given id from the BOM API")]
    public async ValueTask<string> GetPart(
        [Description("The id of the part to load")] int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var requestUri = GetRequestUri(id);
            var part = await this.CreateClient().Get<PartDto>(requestUri, cancellationToken);
            return part is null 
                ? $"Part with Id '{id}' not found." 
                : part.ToString();
        }
        catch (Exception e)
        {
            logger.LogError("Error retrieving part with id {Id}: {Message}", id, e.Message);
            return $"Error retrieving part with id {id}";
        }
    }

    [UsedImplicitly]
    [McpServerTool]
    [Description("Shows the BOM for a part with the given id from the BOM API")]
   public async ValueTask<string> ShowBom(
        [Description("The id of the BOM root part")] int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var requestUri = GetRequestUri(id, "showbom");
            return await this.CreateClient().GetString(requestUri, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError("Error showing BOM for part with id {Id}: {Message}", id, e.Message);
            return $"Error showing BOM for part with id {id}";
        }
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
        try
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
        catch (Exception e)
        {
            logger.LogError("Error creating part with name {Name} and number {Number}: {Message}", name, number, e.Message);
            return $"Error creating part with name {name} and number {number}";
        }
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
        try
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
        catch (Exception e)
        {
            logger.LogError("Updating part with id {Id} failed: {Message}", id, e.Message);
            return $"Updating part with id {id} failed";
        }
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
        try
        {
            var partDto = new PartDto
            {
                Name = name,
                Number = number
            };
        
            var requestUri = GetRequestUri(id, "addsubpart");
            await this.CreateClient().Patch(requestUri, partDto, cancellationToken);
            return "Sub part added successfully.";
        }
        catch (Exception e)
        {
            logger.LogError("Error adding sub part under part with id {Id}: {Message}", id, e.Message);
            return $"Error adding sub part under part with id {id}";
        }
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
        try
        {
            var requestUri = GetRequestUri(id);
            await this.CreateClient().Delete(requestUri, cancellationToken);
            return "Part deleted successfully.";
        }
        catch (Exception e)
        {
            logger.LogError("Error deleting part with id {Id}: {Message}", id, e.Message);
            var message = $"Error deleting part with id {id}";
            return message;
        }
    }
    
    #endregion
    
    #region helper methods

    private static Uri GetRequestUri(int? id = null, string subpath = "")
    {
        var path = "bom";
        if (!string.IsNullOrWhiteSpace(subpath)) path += $"/{subpath.Trim('/')}";
        if (id.HasValue) path += $"/{id.Value}";
        return new Uri(path, UriKind.Relative);
    }
    
    private HttpClient CreateClient()
    {
        return httpClientFactory.CreateClient("BomApiClient");
    }
    
    #endregion
}