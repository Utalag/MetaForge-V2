namespace MetaForge.BusinessModel.Diff;

/// <summary>
/// Druh změny v diffu.
/// </summary>
public enum DiffKind
{
    /// <summary>Položka byla přidána.</summary>
    Added,

    /// <summary>Položka byla odebrána.</summary>
    Removed,

    /// <summary>Položka byla změněna.</summary>
    Modified,
}

/// <summary>
/// Jeden záznam změny v diffu.
/// </summary>
public sealed record DiffEntry
{
    /// <summary>Cesta ke změněné položce (např. "entities/Customer/attributes/Email").</summary>
    public string Path { get; init; } = string.Empty;

    /// <summary>Druh změny.</summary>
    public DiffKind Kind { get; init; }

    /// <summary>Původní hodnota (pro Modified a Removed).</summary>
    public string? OldValue { get; init; }

    /// <summary>Nová hodnota (pro Modified a Added).</summary>
    public string? NewValue { get; init; }
}

/// <summary>
/// Výsledek porovnání dvou stavů dokumentu.
/// </summary>
public sealed record BusinessDocumentDiff
{
    /// <summary>Časové razítko levého (staršího) dokumentu.</summary>
    public DateTimeOffset LeftTimestamp { get; init; }

    /// <summary>Časové razítko pravého (novějšího) dokumentu.</summary>
    public DateTimeOffset RightTimestamp { get; init; }

    /// <summary>Seznam změn.</summary>
    public IReadOnlyList<DiffEntry> Changes { get; init; } = Array.Empty<DiffEntry>();

    /// <summary>Počet změn.</summary>
    public int ChangeCount => Changes.Count;

    /// <summary>Počet přidaných položek.</summary>
    public int AddedCount => Changes.Count(c => c.Kind == DiffKind.Added);

    /// <summary>Počet odebraných položek.</summary>
    public int RemovedCount => Changes.Count(c => c.Kind == DiffKind.Removed);

    /// <summary>Počet změněných položek.</summary>
    public int ModifiedCount => Changes.Count(c => c.Kind == DiffKind.Modified);
}

/// <summary>
/// Porovnává dva stavy BusinessAuthoringDocument a vrací diff.
/// </summary>
public static class BusinessDocumentDiffer
{
    /// <summary>
    /// Porovná dva stavy dokumentu a vrátí seznam změn.
    /// </summary>
    public static BusinessDocumentDiff Diff(
        Models.BusinessAuthoringDocument left,
        Models.BusinessAuthoringDocument right)
    {
        var changes = new List<DiffEntry>();

        // Porovnání entit
        var leftEntityIds = left.Entities.Select(e => e.Id).ToHashSet();
        var rightEntityIds = right.Entities.Select(e => e.Id).ToHashSet();

        // Přidané entity
        foreach (var id in rightEntityIds.Except(leftEntityIds))
        {
            var entity = right.Entities.First(e => e.Id == id);
            changes.Add(new DiffEntry
            {
                Path = $"entities/{entity.Name}",
                Kind = DiffKind.Added,
                NewValue = entity.Name,
            });
        }

        // Odebrané entity
        foreach (var id in leftEntityIds.Except(rightEntityIds))
        {
            var entity = left.Entities.First(e => e.Id == id);
            changes.Add(new DiffEntry
            {
                Path = $"entities/{entity.Name}",
                Kind = DiffKind.Removed,
                OldValue = entity.Name,
            });
        }

        // Změněné entity (atributy)
        foreach (var id in leftEntityIds.Intersect(rightEntityIds))
        {
            var leftEntity = left.Entities.First(e => e.Id == id);
            var rightEntity = right.Entities.First(e => e.Id == id);

            var leftAttrIds = leftEntity.Attributes.Select(a => a.Id).ToHashSet();
            var rightAttrIds = rightEntity.Attributes.Select(a => a.Id).ToHashSet();

            foreach (var attrId in rightAttrIds.Except(leftAttrIds))
            {
                var attr = rightEntity.Attributes.First(a => a.Id == attrId);
                changes.Add(new DiffEntry
                {
                    Path = $"entities/{rightEntity.Name}/attributes/{attr.Name}",
                    Kind = DiffKind.Added,
                    NewValue = attr.Type,
                });
            }

            foreach (var attrId in leftAttrIds.Except(rightAttrIds))
            {
                var attr = leftEntity.Attributes.First(a => a.Id == attrId);
                changes.Add(new DiffEntry
                {
                    Path = $"entities/{leftEntity.Name}/attributes/{attr.Name}",
                    Kind = DiffKind.Removed,
                    OldValue = attr.Type,
                });
            }
        }

        return new BusinessDocumentDiff
        {
            LeftTimestamp = left.LastModified,
            RightTimestamp = right.LastModified,
            Changes = changes.AsReadOnly(),
        };
    }
}
