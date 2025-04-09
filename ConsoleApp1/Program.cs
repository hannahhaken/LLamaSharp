using ChromaDB.Client;
using ConsoleApp1;

//option 1: Use ChromaConnector to abstract Chrome APIS
// var chromaConnector = await ChromaConnector.CreateAsync("http://localhost:8000/api/v1/", "movies");
// chromaConnector.addDocuments();
// chromaConnector.search();


//option 2: make a class with a factory method, that simply returns the Chroma SDK instatiated.
// var collectionClient = await ChromeCollectionFactory.CreateAsync("http://localhost:8000/api/v1/", "movies");

var chromaUri = "http://localhost:8000/api/v1/";
var collectionName = "movies";
var configOptions = new ChromaConfigurationOptions(uri: chromaUri);
using var httpClient = new HttpClient();
var chromaClient = new ChromaClient(configOptions, httpClient);
        
var collection = await chromaClient.GetOrCreateCollection(collectionName);
var collectionClient = new ChromaCollectionClient(collection, configOptions, httpClient);

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

await collectionClient.Add(movieIds, descriptionEmbeddings, metadata);

List<ReadOnlyMemory<float>> queryEmbedding = [new([0.12217915f, -0.034832448f])];

var queryResult = await collectionClient.Query(
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