using System.Diagnostics.CodeAnalysis;
using DocumentFormat.OpenXml.Math;
using LLama.Common;
using LLama.Sampling;
using LLama.Transformers;
using LLama.Native;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Microsoft.SemanticKernel.Memory;

namespace LLama.Examples.Examples;

public class LLama3ChatWithChroma
{
    private const string CollectionName = "sitebulb-docs";

    [Experimental("SKEXP0020")]
    public static async Task Run()
    {
        var modelPath = "/Users/hannahhaken/workarea/LLamaSharp/Meta-Llama-3.1-8B-Instruct-Q3_K_L.gguf";
        var parameters = new ModelParams(modelPath)
        {
            GpuLayerCount = 0,
            PoolingType = LLamaPoolingType.Mean
        };

        using var model = LLamaWeights.LoadFromFile(parameters);
        var embedder = new LLamaEmbedder(model, parameters);

        using var context = model.CreateContext(parameters);
        var executor = new InteractiveExecutor(context);
        
        using var lLamaWeights = model;

        var store = await CreateVectorStore();
        
        var docPaths = new[]
        {
            // "Llama.Examples/Assets/doc1.txt",
            // "Llama.Examples/Assets/doc2.txt"
            "/Users/hannahhaken/workarea/LLamaSharp/LLama.Examples/Assets/doc1.txt",
            "/Users/hannahhaken/workarea/LLamaSharp/LLama.Examples/Assets/doc2.txt"
        };
        await InsertEmbeddingsIntoVectorStore(docPaths, embedder, store);
        
        // var baseHistory = new ChatHistory();
        // ChatSession session = new(executor, baseHistory);
        //
        // session.WithHistoryTransform(new PromptTemplateTransformer(model, withAssistant: true));
        // session.WithOutputTransform(new LLamaTransforms.KeywordTextOutputStreamTransform(
        //     ["User:", "�"], redundancyLength: 5));
        //
        // var inferenceParams = new InferenceParams
        // {
        //     SamplingPipeline = new DefaultSamplingPipeline { Temperature = 0.6f },
        //     MaxTokens = -1,
        //     AntiPrompts = ["User:"]
        // };
        //
        // Console.ForegroundColor = ConsoleColor.Yellow;
        // Console.WriteLine("Manual RAG chat session started.");
        // Console.ForegroundColor = ConsoleColor.Green;
        // Console.Write("User> ");
        // var userInput = Console.ReadLine() ?? "";
        //
        // while (userInput != "exit")
        // {
        //     Console.ForegroundColor = ConsoleColor.White;
        //     Console.Write("Assistant> ");
        //
        //     var queryEmbedding = (await embedder.GetEmbeddings(userInput)).Single();
        //
        //     // var topChunks = store.GetNearestMatchesAsync(
        //     //     collectionName,
        //     //     embedding: queryEmbedding,
        //     //     limit: 3,
        //     //     minRelevanceScore: 0.7
        //     // );
        //     //
        //     // Console.ForegroundColor = ConsoleColor.DarkGray;
        //     // Console.WriteLine("\n[Debug] Top retrieved chunks:");
        //     // foreach (var chunk in topChunksWithScores)
        //     // {
        //     //     Console.WriteLine($"[Score: {chunk.Score:F4}]\n{chunk.chunkText}\n---\n");
        //     // }
        //     //
        //     // Console.ResetColor();
        //     //
        //     // var contextText = string.Join("\n---\n", topChunksWithScores.Select(x => x.chunkText));
        //     // var systemPrompt =
        //     //     "You are a helpful assistant. Use the following documentation to answer. Your answer must remain close to these sentences provided as possible where relevant:\n" +
        //     //     contextText;
        //     //
        //     // // Rebuild the chat history with system prompt first
        //     // var chatHistory = new ChatHistory();
        //     // chatHistory.AddMessage(AuthorRole.System, systemPrompt);
        //     // foreach (var message in baseHistory.Messages)
        //     // {
        //     //     chatHistory.Messages.Add(message);
        //     // }
        //     //
        //     // var newSession = new ChatSession(executor, chatHistory);
        //     // newSession.WithHistoryTransform(new PromptTemplateTransformer(model, withAssistant: true));
        //     // newSession.WithOutputTransform(new LLamaTransforms.KeywordTextOutputStreamTransform(
        //     //     ["User:", "�"], redundancyLength: 5));
        //     //
        //     // await foreach (var text in newSession.ChatAsync(
        //     //                    new ChatHistory.Message(AuthorRole.User, userInput),
        //     //                    inferenceParams))
        //     // {
        //     //     Console.ForegroundColor = ConsoleColor.White;
        //     //     Console.Write(text);
        //     // }
        //     //
        //     // Console.WriteLine();
        //     // Console.ForegroundColor = ConsoleColor.Green;
        //     // Console.Write("User> ");
        //     // userInput = Console.ReadLine() ?? "";
        // }
    }

    [Experimental("SKEXP0020")]
    private static async Task InsertEmbeddingsIntoVectorStore(string[] docPaths, LLamaEmbedder embedder,
        IMemoryStore store)
    {
        foreach (var path in docPaths)
        {
            var text = await File.ReadAllTextAsync(path);
            var chunks = ChunkText(text, 400);
            for (int i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];
                var embedding = (await embedder.GetEmbeddings(chunk)).Single();

                var record = new MemoryRecord(
                    metadata: new MemoryRecordMetadata(
                        isReference: false,
                        id: $"{Path.GetFileName(path)}_chunk_{i}",
                        text: chunk,
                        description: "document chunk",
                        externalSourceName: path,
                        additionalMetadata: ""
                    ),
                    embedding: new ReadOnlyMemory<float>(embedding),
                    key: $"{Path.GetFileName(path)}_chunk_{i}"
                );

                await store.UpsertAsync(CollectionName, record);
            }
        }
    }

    [Experimental("SKEXP0020")]
    private static async Task<IMemoryStore> CreateVectorStore()
    {
        var store = new ChromaMemoryStore("http://localhost:8000");
        await store.CreateCollectionAsync(CollectionName);
        return store;
    }

    private static List<string> ChunkText(string text, int maxLength)
    {
        var chunks = new List<string>();
        for (int i = 0; i < text.Length; i += maxLength)
        {
            var length = Math.Min(maxLength, text.Length - i);
            chunks.Add(text.Substring(i, length));
        }

        return chunks;
    }

    private static float CosineSimilarity(float[] a, float[] b)
    {
        float dot = 0f, normA = 0f, normB = 0f;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        return dot / ((float)Math.Sqrt(normA) * (float)Math.Sqrt(normB));
    }
}