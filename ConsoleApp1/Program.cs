using ConsoleApp1;
using ConsoleApp1.Config;

var config = new LlmConfig(chromaUri: "http://localhost:8000/api/v1/",
    collectionName: "sitebulb-docs",
    llamaModelPath: "/Users/hannahhaken/workarea/LLamaSharp/Meta-Llama-3.1-8B-Instruct-Q3_K_L.gguf");

var llamaEmbedder = new LlamaEmbedderService(config.LlamaModelPath);

await new LlmRunner(llamaEmbedder, config).Run();