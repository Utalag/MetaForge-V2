using System.Text.RegularExpressions;

namespace MetaForge.Translator;

internal static class AiJsonEnvelopeExtractor
{
    private static readonly Regex JsonObjectStringAfterCommaRegex = new(
        ",\\s*\"(?=\\s*[\\[{])",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);

    private static readonly Regex JsonObjectStringAfterColonRegex = new(
        ":\\s*\"(?=\\s*[\\[{])",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);

    private static readonly Regex CodeFenceRegex = new(
        "```(?:\\w+)?\\s*(?<payload>.*?)\\s*```",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);

    private static readonly Regex ThinkBlockRegex = new(
        "<think\\b[^>]*>.*?</think>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);

    private static readonly Regex ReasoningTagRegex = new(
        "</?(?:thinking|reasoning|analysis)\\b[^>]*>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);

    public static string ExtractJsonPayload(string response)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(response);

        var sanitized = RemoveReasoningBlocks(response).Trim();

        if (TryExtractCodeFencePayload(sanitized, out var fencedPayload))
            sanitized = fencedPayload.Trim();

        if (LooksLikeJson(sanitized))
            return sanitized;

        return TryExtractFirstJsonBlock(sanitized, out var jsonPayload)
            ? jsonPayload
            : sanitized;
    }

    public static string RepairCommonJsonIssues(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
            return payload;

        var repaired = JsonObjectStringAfterCommaRegex.Replace(payload, ",");
        repaired = JsonObjectStringAfterColonRegex.Replace(repaired, ":");
        return repaired;
    }

    private static string RemoveReasoningBlocks(string input)
    {
        var sanitized = ThinkBlockRegex.Replace(input, string.Empty);
        sanitized = ReasoningTagRegex.Replace(sanitized, string.Empty);
        return sanitized;
    }

    private static bool TryExtractCodeFencePayload(string input, out string payload)
    {
        var match = CodeFenceRegex.Match(input);
        if (match.Success)
        {
            payload = match.Groups["payload"].Value;
            return true;
        }

        payload = string.Empty;
        return false;
    }

    private static bool LooksLikeJson(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var trimmed = input.Trim();
        return (trimmed.StartsWith('{') && trimmed.EndsWith('}'))
            || (trimmed.StartsWith('[') && trimmed.EndsWith(']'));
    }

    private static bool TryExtractFirstJsonBlock(string input, out string payload)
    {
        payload = string.Empty;

        var startIndex = FindJsonStartIndex(input);
        if (startIndex < 0)
            return false;

        var startChar = input[startIndex];
        var endChar = startChar == '{' ? '}' : ']';
        var depth = 0;
        var inString = false;
        var isEscaped = false;

        for (var index = startIndex; index < input.Length; index++)
        {
            var current = input[index];

            if (inString)
            {
                if (isEscaped)
                {
                    isEscaped = false;
                    continue;
                }

                if (current == '\\')
                {
                    isEscaped = true;
                    continue;
                }

                if (current == '"')
                    inString = false;

                continue;
            }

            if (current == '"')
            {
                inString = true;
                continue;
            }

            if (current == startChar)
            {
                depth++;
                continue;
            }

            if (current != endChar)
                continue;

            depth--;
            if (depth != 0)
                continue;

            payload = input[startIndex..(index + 1)].Trim();
            return true;
        }

        return false;
    }

    private static int FindJsonStartIndex(string input)
    {
        var objectIndex = input.IndexOf('{');
        var arrayIndex = input.IndexOf('[');

        if (objectIndex < 0)
            return arrayIndex;

        if (arrayIndex < 0)
            return objectIndex;

        return Math.Min(objectIndex, arrayIndex);
    }
}