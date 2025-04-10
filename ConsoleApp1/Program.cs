using ConsoleApp1;
using ConsoleApp1.Config;

var config = new LlmConfig(chromaUri: "http://localhost:8000/api/v1/",
    collectionName: "sitebulb-docs",
    llamaModelPath: "/Users/hannahhaken/workarea/LLamaSharp/Meta-Llama-3.1-8B-Instruct-Q3_K_L.gguf");

var llamaEmbedder = new LlamaEmbedderService(config.LlamaModelPath);

// TODO: Make an interface, dependency injection into LlmRunner


//todo:
// Put some of the code in program.cs into the Llm runner class.
// define the dependencies, pass them into the LlmRunner.
// Goal: LlmRunner will NOT instantiate its own dependencies.
new LlmRunner(llamaEmbedder, config).Run();