using ChromaDB.Client;
using ChromaDB.Client.Models;

namespace ConsoleApp1;

// You could NOT expose the Chroma directly.
// Instead, when you walk to communicate with the Vector DB, you use ChromeConnector.

// public class ChromaConnector
// {
//     public ChromaCollectionClient CollectionClient { get; private set; }
//
//     private ChromaConnector(ChromaCollectionClient collectionClient)
//     {
//         CollectionClient = collectionClient;
//     }
//
//     public static async Task<ChromaConnector> CreateAsync(string chromaUri, string collectionName)
//     {
//         var configOptions = new ChromaConfigurationOptions(uri: chromaUri);
//         var httpClient = new HttpClient();
//         var chromaClient = new ChromaClient(configOptions, httpClient);
//         var collection = await chromaClient.GetOrCreateCollection(collectionName);
//         var collectionClient = new ChromaCollectionClient(collection, configOptions, httpClient);
//         return new ChromaConnector(collectionClient);
//     }
//
//     public void addDocuments()
//     {
//         
//     }
//
//     public void search()
//     {
//         
//     }
// }

public class ChromeCollectionFactory
{
    public static async Task<ChromaCollectionClient> CreateAsync(string chromaUri, string collectionName)
    {
        var configOptions = new ChromaConfigurationOptions(uri: chromaUri);
        using var httpClient = new HttpClient();
        var chromaClient = new ChromaClient(configOptions, httpClient);
        
        var collection = await chromaClient.GetOrCreateCollection(collectionName);
        return new ChromaCollectionClient(collection, configOptions, httpClient);
    }
}