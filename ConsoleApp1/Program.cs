using ConsoleApp1.Config;
using ConsoleApp1.Examples;

// ollama.uk.sitebulb.com
// chroma.uk.sitebulb.com

//using var documentExtractor = new DocumentExtractor(LlmConfig.CreateNomicConfig());
//using var documentExtractor = new DocumentExtractor(LlmConfig.CreateExternalNomicConfigForEmbeddings());
//await documentExtractor.AddDocumentationAsync();

//var question = "How do I add my VAT number to Sitebulb invoices?";
var question = "How do I change my sitebulb project name";
await OllamaEmbeddingsAndChromaV2.QueryAsync(LlmConfig.CreateLlama3ConfigWithExternalChroma(), question);

var question2 = "How do I change my project name";
await OllamaEmbeddingsAndChromaV2.QueryAsync(LlmConfig.CreateLlama3ConfigWithExternalChroma(), question2);

//await OllamaEmbeddingsAndChromaV1.Run();
//
//await OllamaEmbeddingsAndChromaV3.Run();
//await OllamaEmbeddingsAndChromaV4.Run();