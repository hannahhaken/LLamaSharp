using ConsoleApp1.Config;
using ConsoleApp1.Utilities;

namespace ConsoleApp1.Examples;

public sealed class DocumentExtractor : IDisposable
{
    private readonly LlmConfig _config;
    private readonly ILlmService _llmService;
    private ChromaService? _chromaService;
    
    public DocumentExtractor(LlmConfig config)
    {
        _config = config;
        _llmService = new OllamaService(config.ModelName);
    }

    public void Dispose()
    {
        _llmService?.Dispose();
        //_chromaService?.Dispose();
    }

    private async Task InitChromaService()
    {
        try
        {
            if (_chromaService is null)
                _chromaService = await ChromaService.CreateAsync(_config.ChromaUri, _config.CollectionName);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task AddDocumentationAsync()
    {
        await InitChromaService();
        await ExtractDocumentChunks();
    }

    private async Task ExtractDocumentChunks()
    {
        await ExtractDocsDocumentChunks();
        await ExtractHintsDocumentChunks();
    }

    private async Task ExtractDocsDocumentChunks()
    {
        await ExtractDocumentChunksByPath("assets/Docs", "HelpCenter");
    }
    
    private async Task ExtractHintsDocumentChunks()
    {
        await ExtractDocumentChunksByPath("assets/Hints", "Hints");
    }

    private async Task ExtractDocumentChunksByPath(string textFileDirectoryPath, string documentType)
    {
        try
        {
            var filePaths = Directory.GetFiles(textFileDirectoryPath, "*.txt");

            Console.WriteLine($"Found {filePaths.Length} files in {textFileDirectoryPath} for {documentType}");

            var total = filePaths.Length;
            int chunkSize = 100; // “tokens” per chunk
            int overlap = 20; // tokens of overlap

            for (var index = 0; index < filePaths.Length; index++)
            {
                var fileStopwatch = ValueStopwatch.StartNew();
                
                var filePath = filePaths[index];
                var fileName = Path.GetFileName(filePath);
                
                Console.WriteLine($"Processing {fileName}: {filePath}. {index + 1} of {total}");
                
                var readingFileStopwatch = ValueStopwatch.StartNew();
                var text = await File.ReadAllTextAsync(filePath);
                Console.WriteLine($"File {fileName} read took {readingFileStopwatch.GetElapsedTime().TotalSeconds}s");
                
                var chunkingStopwatch = ValueStopwatch.StartNew();

                var chunks = TokenTextChunker.ChunkByTokens(text, chunkSize, overlap).ToList();
                Console.WriteLine($"Number of chunks: {chunks.Count}");

                var documentChunks = new List<string>();
                var metadata = new List<Dictionary<string, object>>();
                
                var chunkId = 1;
                foreach (var chunk in chunks)
                {
                    documentChunks.Add(chunk);
                    metadata.Add(new Dictionary<string, object>
                    {
                        ["ChunkId"] = chunkId,
                        ["Source"] = fileName,
                        ["DocumentType"] = documentType,
                        ["Filename"] = Path.GetFileNameWithoutExtension(fileName)
                    });

                    chunkId++;
                }

                var ids = ChunkMetadataBuilder.GenerateDocumentIds(documentChunks.Count, prefix: $"{documentType}-{fileName}");
                
                Console.WriteLine($"File {fileName} chunking took {chunkingStopwatch.GetElapsedTime().TotalSeconds}s");

                try
                {
                    var addDocumentStopwatch = ValueStopwatch.StartNew();
                    await _chromaService!.AddDocumentsAsync(ids, documentChunks, metadata, _llmService);
                    Console.WriteLine($"Upserting File {fileName} took {addDocumentStopwatch.GetElapsedTime().TotalSeconds}s");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to add documents for {fileName}: {ex.Message}");
                }
                finally
                {
                    Console.WriteLine($"File {fileName} took {fileStopwatch.GetElapsedTime().TotalSeconds}s");
                }
            }

            Console.WriteLine("Finished extracting chunks...");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    // public async Task ExtractAssetDocsAsync()
    // {
    //     var config = LlmConfig.CreateLlama3Config();
    //     var llmService = new OllamaService(config.ModelName);
    //     var runner = new LlmRunner(config, llmService); 
    //     await runner.AddDocumentationAsync();
    // }
}