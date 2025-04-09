namespace ConsoleApp1;

public static class ChunkMetadataBuilder
{
    public static List<string> GenerateDocumentIds(int count, string? prefix = null)
    {
        return Enumerable.Range(0, count)
            .Select(i => prefix is null ? i.ToString() : $"{prefix}-{i}")
            .ToList();
    }

    public static List<Dictionary<string, object>> GenerateMetadataForChunks(List<string> chunks, string? source = null)
    {
        return chunks.Select((_, i) =>
        {
            var metadata = new Dictionary<string, object>
            {
                ["ChunkId"] = i
            };

            if (!string.IsNullOrEmpty(source))
                metadata["Source"] = source;

            return metadata;
        }).ToList();
    }
}