using ConsoleApp1.Config;

namespace ConsoleApp1;

public class LlmRunner
{
    private readonly IEmbedderService _embedderService;
    private readonly LlmConfig _config;

    public LlmRunner(IEmbedderService embedderService, LlmConfig llmConfig)
    {
        _embedderService = embedderService;
        _config = llmConfig;
    }

    public async Task Run()
    {
        var chromaService = await ChromaService.CreateAsync(_config.ChromaUri, _config.CollectionName);

        var texts = new List<string>
        {
            "This is the first chunk of Sitebulb hint text.",
            "This is the second chunk of text.",
            "Final chunk for testing."
        };

        var ids = ChunkMetadataBuilder.GenerateDocumentIds(texts.Count, prefix: "sitebulb");
        var metadata = ChunkMetadataBuilder.GenerateMetadataForChunks(texts, source: "sitebulb-docs");

        await chromaService.AddDocumentsAsync(ids, texts, metadata, _embedderService);

        await chromaService.RunSampleQuery(_embedderService);
    }
}