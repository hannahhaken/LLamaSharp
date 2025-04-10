namespace ConsoleApp1.Config;

public class LlmConfig
{
    public string ChromaUri { get; }
    public string CollectionName { get; }
    public string LlamaModelPath { get; }
    
    public LlmConfig(string chromaUri, string collectionName, string llamaModelPath)
    {
        ChromaUri = chromaUri;
        CollectionName = collectionName;
        LlamaModelPath = llamaModelPath;
    }
    
}