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

        var collection = await chromaClient.GetOrCreateCollection(collectionName);
        var collectionClient = new ChromaCollectionClient(collection, configOptions, httpClient);

        return new ChromaService(collectionClient);
    }

    public async Task SeedSampleMovieData()
    {
        List<string> movieIds = ["1", "2", "3", "4", "5"];

        List<ReadOnlyMemory<float>> descriptionEmbeddings =
        [
            new[] { 0.10022575f, -0.23998135f },
            new[] { 0.10327095f, 0.2563685f },
            new[] { 0.095857024f, -0.201278f },
            new[] { 0.106827796f, 0.21676421f },
            new[] { 0.09568083f, -0.21177962f }
        ];

        List<Dictionary<string, object>> metadata =
        [
            new() { ["Title"] = "The Lion King" },
            new() { ["Title"] = "Inception" },
            new() { ["Title"] = "Toy Story" },
            new() { ["Title"] = "Pulp Fiction" },
            new() { ["Title"] = "Shrek" }
        ];

        await _collectionClient.Add(movieIds, descriptionEmbeddings, metadata);
    }

    public async Task RunSampleQuery()
    {
        List<ReadOnlyMemory<float>> queryEmbedding = [new([0.12217915f, -0.034832448f])];

        var queryResult = await _collectionClient.Query(
            queryEmbeddings: queryEmbedding,
            nResults: 2,
            include: ChromaQueryInclude.Metadatas | ChromaQueryInclude.Distances);

        foreach (var result in queryResult)
        {
            foreach (var item in result)
            {
                Console.WriteLine($"Title: {(string)item.Metadata["Title"] ?? string.Empty} {item.Distance}");
            }
        }
    }
}