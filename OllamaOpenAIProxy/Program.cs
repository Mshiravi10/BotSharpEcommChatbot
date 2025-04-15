using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using System.Net.Http;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var ollamaEndpoint = "http://localhost:11434/api/chat";
var http = new HttpClient();

app.Use(async (context, next) =>
{
    Console.WriteLine($"[Proxy] {context.Request.Method} {context.Request.Path}");
    await next();
});

app.MapPost("/v1/chat/completions", async (HttpRequest request) =>
{
    try
    {
        using var reader = new StreamReader(request.Body);
        var jsonString = await reader.ReadToEndAsync();
        var body = JsonSerializer.Deserialize<JsonElement>(jsonString);

        var model = body.GetProperty("model").GetString();
        var messages = body.GetProperty("messages");

        var requestDict = new Dictionary<string, object?>
        {
            ["model"] = model,
            ["messages"] = messages,
            ["stream"] = false
        };

        if (body.TryGetProperty("tools", out var tools))
        {
            var messagesList = messages.EnumerateArray().ToList();
            var lastMessage = messagesList.Last();

            if (lastMessage.GetProperty("role").GetString() == "tool")
            {
                var contentToInject = $"Intent and expected output: {lastMessage.GetProperty("content")}";

                var systemMessage = new Dictionary<string, object?>
                {
                    ["role"] = "system",
                    ["content"] = contentToInject
                };

                messagesList.Insert(0, JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(systemMessage)));

                requestDict["messages"] = messagesList;
            }
            else
            {
                requestDict["tools"] = tools;
            }
        }


        if (body.TryGetProperty("tool_choice", out var toolChoice))
        {
            requestDict["tool_choice"] = toolChoice;
        }

        var messagesList2 = messages.EnumerateArray().ToList();
        for (int i = 0; i < messagesList2.Count; i++)
        {
            var message1 = messagesList2[i];
            if (message1.GetProperty("role").GetString() == "assistant" && string.IsNullOrWhiteSpace(message1.GetProperty("content").GetString()))
            {
                messagesList2[i] = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(new Dictionary<string, object?>
                {
                    ["role"] = "assistant",
                    ["content"] = null
                }));
            }
        }

        requestDict["messages"] = messagesList2;

        var requestJson = JsonSerializer.Serialize(requestDict, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });


        Console.WriteLine("📤 Sent to Ollama:");
        Console.WriteLine(requestJson);

        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await http.PostAsync(ollamaEndpoint, content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var ollamaRaw = JsonSerializer.Deserialize<JsonElement>(responseJson);
        var message = ollamaRaw.GetProperty("message");

        var finalMessage = new Dictionary<string, object?>
        {
            ["role"] = message.GetProperty("role").GetString(),
            ["content"] = message.TryGetProperty("content", out var ce) && ce.ValueKind == JsonValueKind.String
                ? (string.IsNullOrWhiteSpace(ce.GetString()) ? null : ce.GetString())
                : null
        };

        // ✨ Ensure tool_calls are fully formed
        if (message.TryGetProperty("tool_calls", out var toolCalls))
        {
            var toolCallList = new List<Dictionary<string, object?>>();

            foreach (var toolCall in toolCalls.EnumerateArray())
            {
                var function = toolCall.GetProperty("function");
                var name = function.GetProperty("name").GetString();

                var args = function.GetProperty("arguments");
                var argsString = args.ValueKind == JsonValueKind.String
                    ? args.GetString()
                    : JsonSerializer.Serialize(args);

                toolCallList.Add(new Dictionary<string, object?>
                {
                    ["id"] = toolCall.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String
                        ? id.GetString()
                        : Guid.NewGuid().ToString(),
                    ["type"] = "function",
                    ["function"] = new Dictionary<string, object?>
                    {
                        ["name"] = name,
                        ["arguments"] = argsString
                    }
                });
            }

            finalMessage["tool_calls"] = toolCallList;
        }

        var fakeResponse = new
        {
            id = Guid.NewGuid().ToString(),
            @object = "chat.completion",
            created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            model = model,
            choices = new[]
            {
                new
                {
                    index = 0,
                    finish_reason = "stop",
                    message = finalMessage
                }
            },
            usage = new
            {
                prompt_tokens = 0,
                completion_tokens = 0,
                total_tokens = 0
            }
        };

        Console.WriteLine("✅ Final Response to SK:");
        Console.WriteLine(JsonSerializer.Serialize(fakeResponse, new JsonSerializerOptions { WriteIndented = true }));

        return Results.Json(fakeResponse);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Proxy error: {ex.Message}");
        return Results.Problem($"Proxy error: {ex.Message}");
    }
});

app.Run("http://localhost:5111");
