﻿using McpClientConsole;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;

Console.WriteLine("Starting...");

var endpoint = new Uri("http://localhost:5018/");
const string name = "BOM Client";
var timeouts = new TimeoutSettings(120, 60, 30, 60);
ILoggerFactory CreateLogger() => LoggerFactory.Create(builder => builder.AddConsole());
await using var connection = await McpConnection.CreateAndOpen(name, endpoint, timeouts, CreateLogger);

var tools = await connection.GetTools();
if (tools.Count == 0)
{
    Console.WriteLine("No tools available on the server.");
    return;
}

Console.WriteLine($"Found {tools.Count} tools on the server.");

var toolNames = string.Join(",", tools);
Console.WriteLine($"Tools available: {toolNames}");

// Get parts
var result1 = await connection.ExecuteTool("get_parts", new Dictionary<string, object?>());
Console.WriteLine("Result: " + ((TextContentBlock)result1.Content[0]).Text);
Console.WriteLine();

// Add sub part
var result2 = await connection.ExecuteTool("add_sub_part", new Dictionary<string, object?>()
{
    { "id", 11 },
    { "name", "Paint" },
    { "number", "1345" }
});
Console.WriteLine("Result: " + ((TextContentBlock)result2.Content[0]).Text);
Console.WriteLine();

// Show BOM
var result3 = await connection.ExecuteTool("show_bom", new Dictionary<string, object?>()
{
    { "id", 8 }
});
Console.WriteLine("Result:");
Console.WriteLine(((TextContentBlock)result3.Content[0]).Text);
Console.WriteLine();

Console.WriteLine("Done");
Console.ReadLine();