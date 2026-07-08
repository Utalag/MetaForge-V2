namespace MetaForge.Core.Validation;

/// <summary>
/// Strategie generování kombinací pro Class-level testy.
/// </summary>
public enum CombinationStrategy
{
    /// <summary>
    /// Plný kartézský součin všech valid hodnot.
    /// Pozor na exponenciální růst (3 fields × 3 samples = 27).
    /// </summary>
    Full,

    /// <summary>
    /// All-pairs (pairwise) — pokrývá interakce mezi dvěma členy.
    /// Dramaticky méně kombinací při zachování ~99 % pokrytí bugů.
    /// </summary>
    Pairwise,

    /// <summary>
    /// Pouze první valid hodnota z každého členu — minimální smoke test.
    /// </summary>
    FirstOnly
}

/// <summary>
/// Jedna kombinace valid hodnot pro Class-level test.
/// Každý záznam mapuje jméno členu na C# literál.
/// </summary>
public sealed class TestCombination
{
    /// <summary>
    /// Popis kombinace pro test label (např. "Age=18_Name=Alice_Email=a@b.com").
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Mapování: název členu → C# literál.
    /// </summary>
    public IReadOnlyDictionary<string, string> Members { get; }

    public TestCombination(IReadOnlyDictionary<string, string> members)
    {
        Members = members;
        Label = string.Join("_", members.Select(kv => $"{kv.Key}={kv.Value}"));
    }

    public override string ToString() => Label;
}

/// <summary>
/// Profil pro Class-level kombinatorické testy.
/// Bere valid vzorky všech členů a generuje kombinace podle zvolené strategie.
/// </summary>
public sealed class ClassTestProfile
{
    /// <summary>
    /// Strategie generování kombinací.
    /// </summary>
    public CombinationStrategy Strategy { get; init; } = CombinationStrategy.Pairwise;

    /// <summary>
    /// Maximální počet kombinací (hard cap). Default 50.
    /// </summary>
    public int MaxCombinations { get; init; } = 50;

    /// <summary>
    /// Generuje kombinace z valid vzorků členů třídy.
    /// </summary>
    /// <param name="memberSamples">
    /// Slovník: název členu → seznam valid C# literálů.
    /// Např. { "Age": ["0","18","150"], "Name": ["\"Alice\"","\"Bob\""] }
    /// </param>
    public IReadOnlyList<TestCombination> ResolveTestCombinations(
        IReadOnlyDictionary<string, IReadOnlyList<string>> memberSamples)
    {
        if (memberSamples.Count == 0) return [];

        var combinations = Strategy switch
        {
            CombinationStrategy.Full => GenerateFullCartesian(memberSamples),
            CombinationStrategy.Pairwise => GeneratePairwise(memberSamples),
            CombinationStrategy.FirstOnly => GenerateFirstOnly(memberSamples),
            _ => GeneratePairwise(memberSamples)
        };

        return combinations.Take(MaxCombinations).ToList();
    }

    private static List<TestCombination> GenerateFullCartesian(
        IReadOnlyDictionary<string, IReadOnlyList<string>> memberSamples)
    {
        var keys = memberSamples.Keys.ToList();
        var valueLists = keys.Select(k => memberSamples[k]).ToList();

        var results = new List<TestCombination>();
        var indices = new int[keys.Count];

        while (true)
        {
            var dict = new Dictionary<string, string>();
            for (var i = 0; i < keys.Count; i++)
                dict[keys[i]] = valueLists[i][indices[i]];

            results.Add(new TestCombination(dict));

            // Increment indices (odometer style)
            var pos = keys.Count - 1;
            while (pos >= 0)
            {
                indices[pos]++;
                if (indices[pos] < valueLists[pos].Count)
                    break;
                indices[pos] = 0;
                pos--;
            }

            if (pos < 0) break;
        }

        return results;
    }

    private static List<TestCombination> GeneratePairwise(
        IReadOnlyDictionary<string, IReadOnlyList<string>> memberSamples)
    {
        var keys = memberSamples.Keys.ToList();
        var valueLists = keys.Select(k => memberSamples[k]).ToList();
        var paramCount = keys.Count;

        if (paramCount <= 2)
            return GenerateFullCartesian(memberSamples);

        // Všechny povinné páry jako (paramA, valueIdxA, paramB, valueIdxB)
        var uncovered = new HashSet<(int A, int Va, int B, int Vb)>();
        for (var a = 0; a < paramCount; a++)
            for (var b = a + 1; b < paramCount; b++)
                for (var va = 0; va < valueLists[a].Count; va++)
                    for (var vb = 0; vb < valueLists[b].Count; vb++)
                        uncovered.Add((a, va, b, vb));

        var results = new List<TestCombination>();
        var rowIndex = 0;

        while (uncovered.Count > 0)
        {
            var row = new int[paramCount];

            // Seed: každý řádek začíná s jiným rozložením hodnot
            for (var p = 0; p < paramCount; p++)
                row[p] = (rowIndex + p) % valueLists[p].Count;

            // Greedy optimalizace: 2 průchody, obousměrné počítání párů
            for (var pass = 0; pass < 2; pass++)
            {
                for (var p = 0; p < paramCount; p++)
                {
                    var bestValue = row[p];
                    var bestCount = CountPairsCoveredByValue(row, p, row[p], uncovered, paramCount);

                    for (var v = 0; v < valueLists[p].Count; v++)
                    {
                        if (v == bestValue) continue;
                        var count = CountPairsCoveredByValue(row, p, v, uncovered, paramCount);
                        if (count > bestCount)
                        {
                            bestCount = count;
                            bestValue = v;
                        }
                    }

                    row[p] = bestValue;
                }
            }

            // Safety: pokud řádek nepokrývá žádný nový pár, vynuť první nepokrytý
            var coversAny = false;
            for (var a = 0; a < paramCount && !coversAny; a++)
                for (var b = a + 1; b < paramCount && !coversAny; b++)
                    coversAny = uncovered.Contains((a, row[a], b, row[b]));

            if (!coversAny)
            {
                var forced = uncovered.First();
                row[forced.A] = forced.Va;
                row[forced.B] = forced.Vb;
            }

            // Odeber pokryté páry
            for (var a = 0; a < paramCount; a++)
                for (var b = a + 1; b < paramCount; b++)
                    uncovered.Remove((a, row[a], b, row[b]));

            // Sestav TestCombination
            var dict = new Dictionary<string, string>();
            for (var i = 0; i < paramCount; i++)
                dict[keys[i]] = valueLists[i][row[i]];

            results.Add(new TestCombination(dict));
            rowIndex++;
        }

        return results;
    }

    /// <summary>
    /// Počítá kolik nepokrytých párů by pokryla hodnota value na pozici paramIdx,
    /// s ohledem na aktuální přiřazení VŠECH ostatních parametrů (dopředně i zpětně).
    /// </summary>
    private static int CountPairsCoveredByValue(
        int[] row, int paramIdx, int value,
        HashSet<(int A, int Va, int B, int Vb)> uncovered, int paramCount)
    {
        var count = 0;
        for (var other = 0; other < paramCount; other++)
        {
            if (other == paramIdx) continue;

            // Pár je vždy (menší index, větší index)
            int a, va, b, vb;
            if (paramIdx < other)
            { a = paramIdx; va = value; b = other; vb = row[other]; }
            else
            { a = other; va = row[other]; b = paramIdx; vb = value; }

            if (uncovered.Contains((a, va, b, vb)))
                count++;
        }

        return count;
    }

    private static List<TestCombination> GenerateFirstOnly(
        IReadOnlyDictionary<string, IReadOnlyList<string>> memberSamples)
    {
        var dict = new Dictionary<string, string>();
        foreach (var (key, values) in memberSamples)
        {
            if (values.Count > 0)
                dict[key] = values[0];
        }

        return dict.Count > 0 ? [new TestCombination(dict)] : [];
    }
}
