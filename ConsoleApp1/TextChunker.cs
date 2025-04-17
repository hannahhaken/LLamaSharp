namespace ConsoleApp1;

public static class TextChunker
{
    /// <summary>
    /// Splits the input text into chunks of up to <paramref name="chunkSize"/> characters,
    /// with <paramref name="overlap"/> characters overlapping between consecutive chunks.
    /// </summary>
    /// <param name="text">The text to chunk.</param>
    /// <param name="chunkSize">Maximum size of each chunk (must be > 0).</param>
    /// <param name="overlap">Number of characters to overlap between chunks (>= 0, &lt; chunkSize).</param>
    /// <returns>A list of text chunks.</returns>
    /// <exception cref="ArgumentException">Thrown if parameters are out of range.</exception>
    public static List<string> Chunk(string text, int chunkSize, int overlap)
    {
        if (chunkSize <= 0)
            throw new ArgumentException("chunkSize must be > 0", nameof(chunkSize));
        if (overlap < 0)
            throw new ArgumentException("overlap cannot be negative", nameof(overlap));
        if (overlap >= chunkSize)
            throw new ArgumentException("overlap must be less than chunkSize", nameof(overlap));

        var chunks = new List<string>();
        int step = chunkSize - overlap;
        int position = 0;
        while (position < text.Length)
        {
            int length = Math.Min(chunkSize, text.Length - position);
            chunks.Add(text.Substring(position, length));
            position += step;
        }
        return chunks;
    }

    /// <summary>
    /// Streams the chunks lazily rather than building a list.
    /// </summary>
    public static IEnumerable<string> ChunkStream(string text, int chunkSize, int overlap)
    {
        if (chunkSize <= 0)
            throw new ArgumentException("chunkSize must be > 0", nameof(chunkSize));
        if (overlap < 0)
            throw new ArgumentException("overlap cannot be negative", nameof(overlap));
        if (overlap >= chunkSize)
            throw new ArgumentException("overlap must be less than chunkSize", nameof(overlap));

        int step = chunkSize - overlap;
        for (int position = 0; position < text.Length; position += step)
        {
            int length = Math.Min(chunkSize, text.Length - position);
            yield return text.Substring(position, length);
        }
    }
}