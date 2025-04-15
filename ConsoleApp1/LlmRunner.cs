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

        var (documentChunks, metadata) = await ExtractDocumentChunks();

        var ids = ChunkMetadataBuilder.GenerateDocumentIds(documentChunks.Count, prefix: "sitebulb");

        await chromaService.AddDocumentsAsync(ids, documentChunks, metadata, _embedderService);
        await chromaService.RunQuery(_embedderService, _llmService);
    }

    private static async Task<(List<string> documentChunks, List<Dictionary<string, object>> metadata)>
        ExtractDocumentChunks()
    {
        var documentChunks = new List<string>();
        var metadata = new List<Dictionary<string, object>>();
        var filePaths = Directory.GetFiles("assets", "*.txt");

        var chunkId = 0;

        foreach (var filePath in filePaths)
        {
            var text = await File.ReadAllTextAsync(filePath);
            var chunks = DocumentChunker.ChunkText(text, maxLength: 1100);

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
        }

        return (documentChunks, metadata);
    }
}