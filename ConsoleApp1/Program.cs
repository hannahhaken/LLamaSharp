using System.Net.NetworkInformation;
using ConsoleApp1;

const string chromaUri = "http://localhost:8000/api/v1/";
const string collectionName = "sitebulb-docs";
var llamaModelPath = "/Users/hannahhaken/workarea/LLamaSharp/Meta-Llama-3.1-8B-Instruct-Q3_K_L.gguf";

var llamaEmbedder = new LlamaEmbedderService(llamaModelPath);
var chromaService = await ChromaService.CreateAsync(chromaUri, collectionName);

var texts = new List<string>
{
    "This is the first chunk of Sitebulb hint text.",
    "This is the second chunk of text.",
    "Final chunk for testing."
};

var ids = ChunkMetadataBuilder.GenerateDocumentIds(texts.Count, prefix: "sitebulb");
var metadata = ChunkMetadataBuilder.GenerateMetadataForChunks(texts, source: "sitebulb-docs.txt");

await chromaService.AddDocumentsAsync(ids, texts, metadata, llamaEmbedder);

await chromaService.RunSampleQuery(llamaEmbedder);