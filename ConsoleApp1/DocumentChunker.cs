namespace ConsoleApp1;

public static class DocumentChunker
{
    public static List<string> ChunkText(string text, int maxLength = 400)
    {
        var chunks = new List<string>();

        for (var i = 0; i < text.Length; i += maxLength)
        {
            var length = Math.Min(maxLength, text.Length - i);
            chunks.Add(text.Substring(i, length));
        }

        return chunks;
    }
}