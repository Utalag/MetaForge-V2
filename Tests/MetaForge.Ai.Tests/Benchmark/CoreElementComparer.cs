using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Types;

namespace MetaForge.Ai.Tests.Benchmark;

/// <summary>
/// Strukturální komparátor Core elementů pro AI benchmark.
/// Porovnává názvy, typy a strukturu — ignoruje těla metod, XML komentáře, atributy.
/// </summary>
public static class CoreElementComparer
{
    /// <summary>
    /// Vrátí true, pokud oba stromy mají stejnou strukturu (názvy tříd, properties, metody, signatury).
    /// </summary>
    public static bool AreStructurallyEquivalent(ClassElement? reference, ClassElement? candidate)
    {
        if (reference is null && candidate is null) return true;
        if (reference is null || candidate is null) return false;

        var diffs = Diff(reference, candidate);
        return diffs.Count == 0;
    }

    /// <summary>Detailní diff — co přesně chybí/nesouhlasí.</summary>
    public static IReadOnlyList<string> Diff(ClassElement reference, ClassElement candidate)
    {
        var diffs = new List<string>();

        // Název třídy
        if (reference.Name != candidate.Name)
            diffs.Add($"Class name: expected '{reference.Name}', got '{candidate.Name}'");

        // IsStatic
        if (reference.IsStatic != candidate.IsStatic)
            diffs.Add($"Class '{reference.Name}': IsStatic mismatch (expected {reference.IsStatic}, got {candidate.IsStatic})");

        // Properties: porovnat podle názvu a typu
        var refProps = reference.Properties.ToDictionary(p => p.Name);
        var candProps = candidate.Properties.ToDictionary(p => p.Name);

        foreach (var (name, refProp) in refProps)
        {
            if (!candProps.TryGetValue(name, out var candProp))
            {
                diffs.Add($"Class '{reference.Name}': missing property '{name}'");
                continue;
            }

            if (refProp.Type.BaseType != candProp.Type.BaseType)
                diffs.Add($"Class '{reference.Name}': property '{name}' type mismatch (expected {refProp.Type.BaseType}, got {candProp.Type.BaseType})");

            if (refProp.Type.CustomTypeName != candProp.Type.CustomTypeName)
                diffs.Add($"Class '{reference.Name}': property '{name}' custom type mismatch (expected '{refProp.Type.CustomTypeName}', got '{candProp.Type.CustomTypeName}')");
        }

        foreach (var name in candProps.Keys.Except(refProps.Keys))
            diffs.Add($"Class '{reference.Name}': extra property '{name}'");

        // Methods: porovnat podle názvu a signatury
        var refMethods = reference.Methods.GroupBy(m => m.Name).ToDictionary(g => g.Key, g => g.ToList());
        var candMethods = candidate.Methods.GroupBy(m => m.Name).ToDictionary(g => g.Key, g => g.ToList());

        foreach (var (name, refGroup) in refMethods)
        {
            if (!candMethods.TryGetValue(name, out var candGroup))
            {
                diffs.Add($"Class '{reference.Name}': missing method '{name}'");
                continue;
            }

            var refM = refGroup[0];
            var candM = candGroup[0];

            if (refM.ReturnType.BaseType != candM.ReturnType.BaseType)
                diffs.Add($"Class '{reference.Name}': method '{name}' return type mismatch");

            if (refM.Parameters.Count != candM.Parameters.Count)
                diffs.Add($"Class '{reference.Name}': method '{name}' parameter count mismatch ({refM.Parameters.Count} vs {candM.Parameters.Count})");
        }

        foreach (var name in candMethods.Keys.Except(refMethods.Keys))
            diffs.Add($"Class '{reference.Name}': extra method '{name}'");

        return diffs;
    }
}
