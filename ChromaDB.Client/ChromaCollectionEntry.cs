namespace ChromaDB.Client.Models;

public class ChromaCollectionEntry
{
	public string Id { get; }
	public ReadOnlyMemory<float>? Embeddings { get; init; }
	public Dictionary<string, object>? Metadata { get; init; }
	public string? Document { get; init; }
	public List<string?>? Uris { get; init; }
	public dynamic? Data { get; init; }

	public ChromaCollectionEntry(string id)
	{
		Id = id;
	}
}

// public class ChromaCollectionEntry
// {
//     public List<List<string>> Ids { get; set; }
//     public object Embeddings { get; set; } // null in the example, keeping it as object
//     public List<List<string>> Documents { get; set; }
//     public object Uris { get; set; } // null in the example, keeping it as object
//     public List<List<Metadata>> Metadatas { get; set; }
//     public List<List<double>> Distances { get; set; }
//     public List<string> Include { get; set; }
// }
//
// public class Metadata
// {
//     public int ChunkId { get; set; }
//     public string Filename { get; set; }
//     public string Source { get; set; }
// }