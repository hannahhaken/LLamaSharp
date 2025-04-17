using ConsoleApp1.Config;

namespace ConsoleApp1.Examples;

/// <summary>
/// 
/// </summary>
public static class OllamaEmbeddingsAndChromaV2
{
    public static async Task QueryAsync(LlmConfig config, string? query = null)
    {
        var llmService = new OllamaService(config.ModelName, config.OllamaEndpoint);
        var runner = new LlmRunner(config, llmService);
        
        if (string.IsNullOrEmpty(query))
        {
            Console.WriteLine("What would you like to know? Enter your question and press enter.");
            query = Console.ReadLine();
            //var output = "What is a canonical used for?";
            //var output = "Why should I not canonicalize a internal page back to my homepage?";
            //var output = "Why should I not canonicalize a internal page back to my homepage?";

            if (string.IsNullOrEmpty(query))
            {
                Console.WriteLine("You didn't ask a question!");
                return;
            }
        }
        
        await runner.ExecutePromptAsync(query);
    }
}