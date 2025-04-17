// using ConsoleApp1.Config;
//
// namespace ConsoleApp1.Examples;
//
// /// <summary>
// /// 
// /// </summary>
// public static class OllamaEmbeddingsAndChromaV1
// {
//     public static async Task Run()
//     {
//         var config = new LlmConfig(
//             chromaUri: "http://localhost:8000/api/v2/tenants/default_tenant/databases/default_database/",
//             collectionName: "sitebulb-docs",
//             llamaModelPath: "/Users/hannahhaken/workarea/LLamaSharp/Meta-Llama-3.1-8B-Instruct-Q3_K_L.gguf",
//             modelName: "llama3");
//
//         //var llmService = new LlamaChatService(config.LlamaModelPath);
//         var llmService = new OllamaService(config.ModelName);
//         var llamaEmbedder = new LlamaEmbedderService(config.LlamaModelPath);
//         var runner = new LlmRunner(config, llmService);
//
// //await runner.AddDocumentationAsync();
//
//         Console.WriteLine("What would you like to know?");
// //var output = Console.ReadLine();
//
// //var output = "What is a canonical used for?";
// //var output = "Why should I not canonicalize a internal page back to my hompage?";
//         var output = "Why should I not canonicalize a internal page back to my hompage?";
//
//         await runner.ExecutePromptAsync(output);
//     }
// }