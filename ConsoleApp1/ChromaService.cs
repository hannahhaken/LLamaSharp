using System.Text;
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

    public async Task AddDocumentsAsync(List<string> ids, List<string> chunks,
        List<Dictionary<string, object>> metadata, IEmbedderService embedder)
    {
        var embeddings = new List<ReadOnlyMemory<float>>();

        foreach (var chunk in chunks)
        {
            var embedding = await embedder.GetEmbeddingsAsync(chunk);
            embeddings.Add(new ReadOnlyMemory<float>(embedding));
        }

        if (ids.Count != chunks.Count || chunks.Count != embeddings.Count || embeddings.Count != metadata.Count)
            throw new InvalidOperationException("Mismatched input sizes for Upsert.");

        await _collectionClient.Upsert(ids, embeddings, metadata, chunks);
    }

    public async Task RunQuery(IEmbedderService embedder, ILlmService llmService)
    {
        const string userQuery =
            "What are redirects?";

        var queryEmbedding = await embedder.GetEmbeddingsAsync(userQuery);

        var result = await _collectionClient.Query(
            queryEmbeddings: [new ReadOnlyMemory<float>(queryEmbedding)],
            nResults: 3,
            include: ChromaQueryInclude.Documents | ChromaQueryInclude.Metadatas | ChromaQueryInclude.Distances
        );

        var contextBuilder = new StringBuilder();

        foreach (var item in result.SelectMany(r => r))
        {
            var docText = item.Document;
            var title = item.Metadata.TryGetValue("Source", out var t)
                ? t.ToString()
                : "(no title)";
            contextBuilder.AppendLine($"From {title}:\n{docText}\n");
            Console.WriteLine($"Distance: {item.Distance:F4} |  ID: {item.Id}. |Document: {item.Document}");
        }

        var context = contextBuilder.ToString();
        Console.WriteLine(context);

        var prompt =
            $"You are a helpful SEO assistant. Use the following documentation to answer the question below. Your answer must remain close to these sentences provided as possible where relevant\n\n" +
            $"Documentation:\n{context}\n\n" +
            $"Question: {userQuery}\n\n";

        Console.WriteLine($"Prompt: {prompt}");
        var response = await llmService.GetChatResponse(prompt);

        Console.WriteLine("\n--- LLM Response ---\n");
        Console.WriteLine(response);
    }
}