using System.Collections.Frozen;
using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Validation;

/// <summary>
/// Centrální registr testovacích profilů pro všechny DataType.
/// Obsahuje předdefinované valid/invalid hodnoty.
/// Uživatel může přepsat per-field: registry.Get(DataType.Email) with { Invalid = ... }.
/// </summary>
public static class DataTypeTestRegistry
{
    private static readonly FrozenDictionary<DataType, DataTypeTestProfile> Defaults = BuildDefaults();

    /// <summary>
    /// Vrátí default profil pro daný DataType.
    /// Pokud DataType nemá specifický profil, vrátí prázdný.
    /// </summary>
    public static DataTypeTestProfile Get(DataType dataType)
        => Defaults.GetValueOrDefault(dataType, new DataTypeTestProfile { DataType = dataType });

    private static FrozenDictionary<DataType, DataTypeTestProfile> BuildDefaults()
    {
        var profiles = new Dictionary<DataType, DataTypeTestProfile>
        {
            // ── Numerické typy ──────────────────────────────────────────

            [DataType.Byte] = new()
            {
                DataType = DataType.Byte,
                Valid = new() { Samples = ["0", "127", "255"] },
                Invalid = new()
                {
                    Range = new RangeConstraint(0, 255),
                    Nullability = NullConstraints.ForbidNull
                }
            },

            [DataType.Short] = new()
            {
                DataType = DataType.Short,
                Valid = new() { Samples = ["0", "-1", "32767"] },
                Invalid = new()
                {
                    Range = new RangeConstraint(short.MinValue, short.MaxValue),
                    Nullability = NullConstraints.ForbidNull
                }
            },

            [DataType.Int] = new()
            {
                DataType = DataType.Int,
                Valid = new() { Samples = ["0", "-1", "42", "int.MaxValue"] },
                Invalid = new()
                {
                    Nullability = NullConstraints.ForbidNull
                }
            },

            [DataType.Long] = new()
            {
                DataType = DataType.Long,
                Valid = new() { Samples = ["0L", "-1L", "long.MaxValue"] },
                Invalid = new()
                {
                    Nullability = NullConstraints.ForbidNull
                }
            },

            [DataType.Float] = new()
            {
                DataType = DataType.Float,
                Valid = new() { Samples = ["0f", "3.14f", "-1.5f"] },
                Invalid = new()
                {
                    ForbiddenLiterals = ["float.NaN", "float.PositiveInfinity", "float.NegativeInfinity"],
                    Nullability = NullConstraints.ForbidNull
                }
            },

            [DataType.Double] = new()
            {
                DataType = DataType.Double,
                Valid = new() { Samples = ["0d", "3.14159", "-1.5"] },
                Invalid = new()
                {
                    ForbiddenLiterals = ["double.NaN", "double.PositiveInfinity", "double.NegativeInfinity"],
                    Nullability = NullConstraints.ForbidNull
                }
            },

            [DataType.Decimal] = new()
            {
                DataType = DataType.Decimal,
                Valid = new() { Samples = ["0m", "99.99m", "-1000m"] },
                Invalid = new()
                {
                    Nullability = NullConstraints.ForbidNull
                }
            },

            // ── Logické typy ────────────────────────────────────────────

            [DataType.Boolean] = new()
            {
                DataType = DataType.Boolean,
                Valid = new() { Samples = ["true", "false"] },
                Invalid = new()
                {
                    Nullability = NullConstraints.ForbidNull
                }
            },

            // ── Textové typy ────────────────────────────────────────────

            [DataType.String] = new()
            {
                DataType = DataType.String,
                Valid = new() { Samples = ["\"hello\"", "\"world\"", "\"test string\""] },
                Invalid = new()
                {
                    Nullability = NullConstraints.All
                }
            },

            [DataType.Char] = new()
            {
                DataType = DataType.Char,
                Valid = new() { Samples = ["'a'", "'Z'", "'0'"] },
                Invalid = new()
                {
                    ForbiddenLiterals = ["'\\0'"],
                    Nullability = NullConstraints.ForbidNull
                }
            },

            // ── Speciální typy ──────────────────────────────────────────

            [DataType.Guid] = new()
            {
                DataType = DataType.Guid,
                Valid = new() { Samples = ["Guid.NewGuid()", "Guid.Parse(\"550e8400-e29b-41d4-a716-446655440000\")"] },
                Invalid = new()
                {
                    ForbiddenLiterals = ["Guid.Empty"],
                    Nullability = NullConstraints.ForbidNull
                }
            },

            // ── Časové typy ─────────────────────────────────────────────

            [DataType.Date] = new()
            {
                DataType = DataType.Date,
                Valid = new() { Samples = ["DateOnly.FromDateTime(DateTime.Now)", "new DateOnly(2024, 1, 15)"] },
                Invalid = new()
                {
                    ForbiddenLiterals = ["DateOnly.MinValue"],
                    Nullability = NullConstraints.ForbidNull
                }
            },

            [DataType.Time] = new()
            {
                DataType = DataType.Time,
                Valid = new() { Samples = ["TimeOnly.FromDateTime(DateTime.Now)", "new TimeOnly(14, 30)"] },
                Invalid = new()
                {
                    Nullability = NullConstraints.ForbidNull
                }
            },

            [DataType.DateTime] = new()
            {
                DataType = DataType.DateTime,
                Valid = new() { Samples = ["DateTime.UtcNow", "new DateTime(2024, 6, 15, 10, 30, 0)"] },
                Invalid = new()
                {
                    ForbiddenLiterals = ["DateTime.MinValue"],
                    Nullability = NullConstraints.ForbidNull
                }
            },
        };

        return profiles.ToFrozenDictionary();
    }
}
