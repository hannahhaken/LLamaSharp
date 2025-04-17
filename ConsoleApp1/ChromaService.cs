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
        
        // TODO: Why does this not work.
        // await chromaClient.DeleteCollection(collectionName);

        var collection = await chromaClient.GetOrCreateCollection(collectionName);
        var collectionClient = new ChromaCollectionClient(collection, configOptions, httpClient);

        return new ChromaService(collectionClient);
    }
    
    public async Task AddDocumentsAsync(List<string> ids, List<string> chunks, List<Dictionary<string, object>> metadata, ILlmService llmService)
    {
        var embeddings = new List<ReadOnlyMemory<float>>();

        foreach (var chunk in chunks)
        {
            var embedding = await llmService.GetEmbeddings(chunk, "nomic-embed-text");
            embeddings.Add(embedding.Embeddings);
        }

        if (ids.Count != chunks.Count || chunks.Count != embeddings.Count || embeddings.Count != metadata.Count)
            throw new InvalidOperationException("Mismatched input sizes for Upsert.");
        
        await _collectionClient.Add(ids, embeddings, metadata, chunks);
    }

    public async Task UpsertDocumentsAsync(List<string> ids, List<string> chunks, List<Dictionary<string, object>> metadata, ILlmService llmService)
    {
        var embeddings = new List<ReadOnlyMemory<float>>();

        foreach (var chunk in chunks)
        {
            var embedding = await llmService.GetEmbeddings(chunk, "nomic-embed-text");
            embeddings.Add(embedding.Embeddings);
        }

        if (ids.Count != chunks.Count || chunks.Count != embeddings.Count || embeddings.Count != metadata.Count)
            throw new InvalidOperationException("Mismatched input sizes for Upsert.");
        
        await _collectionClient.Upsert(ids, embeddings, metadata, chunks);
    }

    public async Task RunQuery(string userQuery, IEmbedderService embedder, ILlmService llmService)
    {
        var queryEmbedding = await llmService.GetEmbeddings(userQuery, "nomic-embed-text");

        //var queryEmbedding = await embedder.GetEmbeddingsAsync(userQuery);
        
        var result = await _collectionClient.Query(
            queryEmbeddings: [queryEmbedding.Embeddings],
            nResults: 20,
            include: ChromaQueryInclude.Documents | ChromaQueryInclude.Metadatas | ChromaQueryInclude.Distances
        );

        var contextBuilder = new StringBuilder();
        
        foreach (var item in result.SelectMany(r => r))
        {
            var docText = item.Document;
            contextBuilder.AppendLine(docText);
            contextBuilder.AppendLine("");
            
            // var title = item.Metadata.TryGetValue("Source", out var t)
            //     ? t.ToString()
            //     : "(no title)";
            // Console.WriteLine($"Title: {title} | Distance: {item.Distance:F4} |  ID: {item.Id}. |Document: {item.Document}");
        }

        // foreach (var item in result.SelectMany(r => r))
        // {
        //     var docText = item.Document;
        //     var title = item.Metadata.TryGetValue("Source", out var t)
        //         ? t.ToString()
        //         : "(no title)";
        //     contextBuilder.AppendLine($"From {title}:\n{docText}\n");
        //     Console.WriteLine($"Distance: {item.Distance:F4} |  ID: {item.Id}. |Document: {item.Document}");
        // }

        var context = contextBuilder.ToString();
        //Console.WriteLine(context);

        var prompt =
            $"\n\nYou are a helpful Sitebulb Support Assistant. Use the following documentation to answer your user's question below. " +
            $"If required you can answer the question with the steps to take; otherwise just answer the question.\n\n" +
            //$"Use the following documentation to answer the question below. Your answer must remain close to these sentences provided as possible where relevant\n\n" +
            $"Documentation:\n{context}\n\n" +
            $"Question: {userQuery}\n\n";
        
        

        //Console.WriteLine($"Prompt: {prompt}");
        var response = await llmService.GetChatResponse(prompt, "llama3");

        Console.WriteLine($"Question: {userQuery}\n\n");
        Console.WriteLine($"Answer: {response}\n\n");
        
        var prompt2 =
            "You are a helpful, professional Sitebulb support assistant. \n" +
            "The user will always be asking a question about Sitebulb or Sitebulb Ltd the business. \n" +
            "Always reply in a friendly manner, as if you were talking to a friend. \n" +
            "Always ask them to add a follow up question, if your response didn't answer their original question. \n" +
            "Use the documentation below to answer the user's question. \n" +
            "Always use the documentation when it contains a relevant answer. \n" +
            "Do not mention that you are reading from the documentation. \n" +
            "If instructions are appropriate, explain the steps clearly.\n" +
            "Do not guess if the documentation does not support an answer.\n" +
            "If you cannot answer the question, give the user our help centre URL https://support.sitebulb.com/en/\n\n" +
            $"Documentation:\n{context}\n\n" +
            $"User Question:\n{userQuery}\n\n" +
            "Answer:";
        
        //Console.WriteLine($"Prompt: {prompt2}");
        var response2 = await llmService.GetChatResponse(prompt2, "llama3");

        Console.WriteLine($"Question: {userQuery}\n\n");
        Console.WriteLine($"Answer:\n\n {response2}\n\n");
    }
}