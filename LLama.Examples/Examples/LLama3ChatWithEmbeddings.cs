using LLama.Common;
using LLama.Sampling;
using LLama.Transformers;
using LLama.Native;

namespace LLama.Examples.Examples;

public class LLama3ChatWithEmbeddings
{
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

        var documentChunks = new List<(string chunkText, float[] embedding)>();

        var docPaths = new[]
        {
            "Llama.Examples/Assets/doc1.txt",
            "Llama.Examples/Assets/doc2.txt"
        };
        foreach (var path in docPaths)
        {
            var text = await File.ReadAllTextAsync(path);
            var chunks = ChunkText(text, 400);
            foreach (var chunk in chunks)
            {
                var embedding = (await embedder.GetEmbeddings(chunk)).Single();
                documentChunks.Add((chunk, embedding));
            }
        }

        var baseHistory = new ChatHistory();
        ChatSession session = new(executor, baseHistory);

        session.WithHistoryTransform(new PromptTemplateTransformer(model, withAssistant: true));
        session.WithOutputTransform(new LLamaTransforms.KeywordTextOutputStreamTransform(
            ["User:", "�"], redundancyLength: 5));

        var inferenceParams = new InferenceParams
        {
            SamplingPipeline = new DefaultSamplingPipeline { Temperature = 0.6f },
            MaxTokens = -1,
            AntiPrompts = ["User:"]
        };

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Manual RAG chat session started.");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("User> ");
        var userInput = Console.ReadLine() ?? "";

        while (userInput != "exit")
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Assistant> ");

            var queryEmbedding = (await embedder.GetEmbeddings(userInput)).Single();
            var topChunksWithScores = documentChunks
                .Select(dc => new { dc.chunkText, Score = CosineSimilarity(queryEmbedding, dc.embedding) })
                .OrderByDescending(x => x.Score)
                .Take(3)
                .ToList();

            // Print top retrieved chunks for debugging
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\n[Debug] Top retrieved chunks:");
            foreach (var chunk in topChunksWithScores)
            {
                Console.WriteLine($"[Score: {chunk.Score:F4}]\n{chunk.chunkText}\n---\n");
            }

            Console.ResetColor();

            var contextText = string.Join("\n---\n", topChunksWithScores.Select(x => x.chunkText));
            var systemPrompt = "You are a helpful assistant. Use the following documentation to answer. Your answer must remain close to these sentences provided as possible where relevant:\n" + contextText;

            // Rebuild the chat history with system prompt first
            var chatHistory = new ChatHistory();
            chatHistory.AddMessage(AuthorRole.System, systemPrompt);
            foreach (var message in baseHistory.Messages)
            {
                chatHistory.Messages.Add(message);
            }
            
            var newSession = new ChatSession(executor, chatHistory);
            newSession.WithHistoryTransform(new PromptTemplateTransformer(model, withAssistant: true));
            newSession.WithOutputTransform(new LLamaTransforms.KeywordTextOutputStreamTransform(
                ["User:", "�"], redundancyLength: 5));

            await foreach (var text in newSession.ChatAsync(
                               new ChatHistory.Message(AuthorRole.User, userInput),
                               inferenceParams))
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(text);
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("User> ");
            userInput = Console.ReadLine() ?? "";
        }
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