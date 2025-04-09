using ConsoleApp1;

const string chromaUri = "http://localhost:8000/api/v1/";
const string collectionName = "movies";

var chromaService = await ChromaService.CreateAsync(chromaUri, collectionName);
await chromaService.SeedSampleMovieData();
await chromaService.RunSampleQuery();