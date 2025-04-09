using ChromaDB.Client;

namespace ConsoleApp1;

public class ChromaService
{
    private readonly ChromaCollectionClient _collectionClient;

    private ChromaService(ChromaCollectionClient collectionClient)
    {
        _collectionClient = collectionClient;
    }

    public static async Task<ChromaService> CreateAsync(string chromaUri, string collectionName)
    {
        var configOptions = new ChromaConfigurationOptions(uri: chromaUri);
        var httpClient = new HttpClient();
        var chromaClient = new ChromaClient(configOptions, httpClient);

        // await chromaClient.DeleteCollection(collectionName);

        var collection = await chromaClient.GetOrCreateCollection(collectionName);
        var collectionClient = new ChromaCollectionClient(collection, configOptions, httpClient);

        return new ChromaService(collectionClient);
    }

    public async Task AddDocumentsAsync(List<string> ids, List<string> texts, List<Dictionary<string, object>> metadata,
        LlamaEmbedderService embedder)
    {
        var embeddings = new List<ReadOnlyMemory<float>>();

        foreach (var text in texts)
        {
            var embedding = await embedder.GetEmbeddingsAsync(text);
            embeddings.Add(new ReadOnlyMemory<float>(embedding));
        }

        await _collectionClient.Add(ids, embeddings, metadata);
    }

    public async Task RunSampleQuery(LlamaEmbedderService embedder)
    {
        var userQuery = "What are the key takeaways?";
        var queryEmbedding = await embedder.GetEmbeddingsAsync(userQuery);

        var result = await _collectionClient.Query(
            queryEmbeddings: [new ReadOnlyMemory<float>(queryEmbedding)],
            nResults: 3,
            include: ChromaQueryInclude.Metadatas | ChromaQueryInclude.Distances
        );

        foreach (var item in result.SelectMany(r => r))
        {
            var title = item.Metadata.TryGetValue("Title", out var t) ? t.ToString() : "(no title)";
            Console.WriteLine($"🔍 Match: {title} | Distance: {item.Distance:F4}");
        }
    }
}