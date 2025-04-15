using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.DependencyInjection;
using BotSharpEcommChatbot.Console.Functions;

var builder = Kernel.CreateBuilder();

#pragma warning disable SKEXP0010
builder.Services.AddSingleton<IChatCompletionService>(new OpenAIChatCompletionService(
    modelId: "llama3-groq-tool-use",
    endpoint: new Uri("http://localhost:5111/v1"),
    apiKey: "",
    organization: null,
    httpClient: new HttpClient()
));
#pragma warning restore SKEXP0010



var kernel = builder.Build();

kernel.ImportPluginFromObject(new ProductFunctions(), "product");

Console.WriteLine("🛒 Agent is ready. Ask a question:");

var chat = kernel.GetRequiredService<IChatCompletionService>() as OpenAIChatCompletionService;
var chatHistory = new ChatHistory();

while (true)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("You: ");
    Console.ResetColor();

    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input)) break;

    chatHistory.AddUserMessage(input);

    var result = await chat.GetChatMessageContentAsync(chatHistory, new OpenAIPromptExecutionSettings
    {
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,        
    }, kernel);

    chatHistory.AddMessage(result.Role, result.Content!);

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"🤖 Agent: {result.Content}");
    Console.ResetColor();
}
