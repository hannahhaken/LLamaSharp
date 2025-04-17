namespace ConsoleApp1.Config;

public class LlmConfig
{
    public string ChromaUri { get; }
    public string OllamaEndpoint { get; }
    public string CollectionName { get; }
    public string ModelName { get; }
    
    public LlmConfig(string chromaUri, string collectionName, string ollamaEndpoint, string modelName)
    {
        OllamaEndpoint = ollamaEndpoint;
        ChromaUri = chromaUri;
        CollectionName = collectionName;
        ModelName = modelName;
    }
    
    public static LlmConfig CreateLlama3Config(string collectionName)
    {
        return new LlmConfig(
            chromaUri: "http://localhost:8000/api/v2/tenants/default_tenant/databases/default_database/",
            collectionName: collectionName,
            ollamaEndpoint: "http://localhost:11434",
            modelName: "llama3");
    } 

    public static LlmConfig CreateNomicEmbeddingsConfig()
    {
        return new LlmConfig(
            chromaUri: "http://localhost:8000/api/v2/tenants/default_tenant/databases/default_database/",
            collectionName: "sitebulb-nomic-docs",
            ollamaEndpoint: "http://localhost:11434",
            modelName: "nomic-embed-text");
    }    
    
    public static LlmConfig CreateExternalNomicConfigForEmbeddings()
    {
        return new LlmConfig(
            chromaUri: "https://chroma.uk.sitebulb.com/api/v2/tenants/default_tenant/databases/default_database/",
            collectionName: "sitebulb-docs",
            ollamaEndpoint: "https://ollama.uk.sitebulb.com",
            modelName: "nomic-embed-text");
    }
    
    public static LlmConfig CreateLlama3ConfigWithExternalChroma()
    {
        return new LlmConfig(
            chromaUri: "https://chroma.uk.sitebulb.com/api/v2/tenants/default_tenant/databases/default_database/",
            collectionName: "sitebulb-docs",
            ollamaEndpoint: "https://ollama.uk.sitebulb.com",
            modelName: "llama3");
    }
}