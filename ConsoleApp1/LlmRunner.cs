using ConsoleApp1.Config;

namespace ConsoleApp1;

public class LlmRunner
{
    private readonly IEmbedderService _embedderService;
    private readonly LlmConfig _config;
    private readonly ILlmService _llmService;

    public LlmRunner(IEmbedderService embedderService, LlmConfig llmConfig, ILlmService llmService)
    {
        _embedderService = embedderService;
        _config = llmConfig;
        _llmService = llmService;
    }

    public async Task Run()
    {
        var chromaService = await ChromaService.CreateAsync(_config.ChromaUri, _config.CollectionName);

        await ExtractDocumentChunks(chromaService, _embedderService);

        await chromaService.RunQuery(_embedderService, _llmService);
    }

    private static async Task ExtractDocumentChunks(ChromaService chromaService, IEmbedderService embedderService)
    {
        var filePaths = Directory.GetFiles("assets", "*.txt");

        var chunkId = 0;

        Console.WriteLine($"Number of files found: {filePaths.Length}");

        foreach (var filePath in filePaths)
        {
            Console.WriteLine($"Processing file: {filePath}");
            var text = await File.ReadAllTextAsync(filePath);

            var chunks = DocumentChunker.ChunkText(text, maxLength: 1100);
            Console.WriteLine($"Number of chunks: {chunks.Count}");

            var documentChunks = new List<string>();
            var metadata = new List<Dictionary<string, object>>();

            var fileName = Path.GetFileName(filePath);

            foreach (var chunk in chunks)
            {
                documentChunks.Add(chunk);
                metadata.Add(new Dictionary<string, object>
                {
                    ["ChunkId"] = chunkId,
                    ["Source"] = fileName,
                    ["Filename"] = Path.GetFileNameWithoutExtension(fileName)
                });

                chunkId++;
            }

            var ids = ChunkMetadataBuilder.GenerateDocumentIds(documentChunks.Count, prefix: "sitebulb");

            try
            {
                Console.WriteLine($"Found {ids.Count} ids");
                await chromaService.AddDocumentsAsync(ids, documentChunks, metadata, embedderService);
                Console.WriteLine($"Added {ids.Count} ids");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to add documents for {fileName}: {ex.Message}");
            }
        }

        Console.WriteLine("Finished extracting chunks...");
    }
}