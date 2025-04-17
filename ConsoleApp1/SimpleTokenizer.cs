using System.Text.RegularExpressions;

namespace ConsoleApp1;

/// <summary>
/// A very simple tokenizer: splits on whitespace and common punctuation,
/// keeps the delimiters as separate tokens.
/// </summary>
public class SimpleTokenizer
{
    // This regex will split out whitespace and punctuation characters into their own tokens.
    // Adjust the pattern if you want different behavior.
    private static readonly Regex _splitRegex = new Regex(@"(\s+|[.,!?;:()\[\]{}""'])",
        RegexOptions.Compiled);

    public List<string> Tokenize(string text)
    {
        return _splitRegex
            .Split(text)
            .Where(t => t.Length > 0)
            .ToList();
    }

    public string Detokenize(IList<string> tokens)
    {
        // Simply concatenate. You could insert a space between tokens that are letters/words
        // if you prefer, e.g. join with string.Empty or with " ".
        return string.Concat(tokens);
    }
}

/// <summary>
/// Chunks text by tokenizer‐defined “tokens” rather than raw characters.
/// </summary>
public static class TokenTextChunker
{
    /// <summary>
    /// Splits text into chunks of up to chunkSize tokens, with overlap tokens of overlap.
    /// </summary>
    public static IEnumerable<string> ChunkByTokens(
        string text,
        int chunkSize,
        int overlap,
        SimpleTokenizer tokenizer = null
    )
    {
        if (chunkSize <= 0) throw new ArgumentException("chunkSize must be > 0");
        if (overlap < 0) throw new ArgumentException("overlap cannot be negative");
        if (overlap >= chunkSize)
            throw new ArgumentException("overlap must be less than chunkSize");

        tokenizer ??= new SimpleTokenizer();
        var tokens = tokenizer.Tokenize(text);
        int step = chunkSize - overlap;

        for (int i = 0; i < tokens.Count; i += step)
        {
            int len = Math.Min(chunkSize, tokens.Count - i);
            var slice = tokens.Skip(i).Take(len).ToList();
            yield return tokenizer.Detokenize(slice);
        }
    }
}