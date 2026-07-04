using System.Text.Json;

namespace MetaForge.BusinessModel;

public sealed class JsonlShadowCommandReader
{
    public JsonlShadowCommandReader(string? filePath = null)
    {
        FilePath = string.IsNullOrWhiteSpace(filePath)
            ? JsonlShadowCommandStore.GetDefaultPath()
            : Path.GetFullPath(filePath);
    }

    public string FilePath { get; }

    public IReadOnlyList<CommandEnvelope> ReadAll()
    {
        if (!File.Exists(FilePath))
            return [];

        var envelopes = new List<CommandEnvelope>();
        var lineNumber = 0;

        foreach (var line in File.ReadLines(FilePath))
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var envelope = JsonSerializer.Deserialize<CommandEnvelope>(line)
                ?? throw new InvalidOperationException($"Shadow log radek {lineNumber} je prazdny nebo nevalidni.");

            envelopes.Add(envelope);
        }

        return envelopes;
    }
}