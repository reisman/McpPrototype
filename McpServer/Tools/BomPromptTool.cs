using System.ComponentModel;
using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[UsedImplicitly]
[McpServerPromptType]
[Description("Prompts for BOM Tool for reading, creating and deleting of parts from the BOM API")]
public sealed class BomPromptTool
{
    #region loading

    [UsedImplicitly]
    [McpServerPrompt]
    [Description("Prompt for reads all available parts from the BOM API")]
    public ChatMessage GetParts()
    {
        return new ChatMessage(ChatRole.User, "Show me all parts");
    }

    [UsedImplicitly]
    [McpServerPrompt]
    [Description("Prompt for read a single part with the given id from the BOM API")]
    public ChatMessage GetPart(int id)
    {
        return new ChatMessage(ChatRole.User, $"Show me all information for part with id {id}");
    }

    #endregion

    #region creation

    [UsedImplicitly]
    [McpServerPrompt]
    [Description("Creates a new part with the given name and number using the BOM API")]
    public ChatMessage CreatePart(string name, string number)
    {
        return new ChatMessage(ChatRole.User, $"Create a new part with name '{name}' and number '{number}'");
    }

    #endregion

    #region updating

    [UsedImplicitly]
    [McpServerPrompt]
    [Description("Prompt for update a new part with the given name and number using the BOM API")]
    public ChatMessage UpdatePart(int id, string name, string number)
    {
        return new ChatMessage(ChatRole.User,
            $"Update the part with id '{id}' to have name '{name}' and number '{number}'");
    }

    [UsedImplicitly]
    [McpServerPrompt]
    [Description("Prompt for adding a sub part with the given name and number using the BOM API")]
    public ChatMessage AddSubPart(int id, string name, string number)
    {
        return new ChatMessage(ChatRole.User, $"Add a new sub part below part with id '{id}' to have name '{name}' and number '{number}'");
    }
    #endregion

    #region deletion

    [UsedImplicitly]
    [McpServerPrompt]
    [Description("Prompt for delete a part with the given id using the BOM API")]
    public ChatMessage DeletePart(int id)
    {
        return new ChatMessage(ChatRole.User, $"Delete the part with id '{id}'");
    }

    #endregion
}