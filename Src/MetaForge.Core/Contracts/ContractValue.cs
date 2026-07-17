// ---------------------------------------------------------------------------
// MetaForge.Core — ContractValue
// Typed value carrier for contract scenarios and verification.
// Vrstva: Core / Contracts
//
// PROPOSAL: PROP-057 — ElementContract + VerificationModel
// ---------------------------------------------------------------------------

using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Contracts;

/// <summary>
/// Typovaný nosič konkrétní hodnoty pro kontrakty a verifikační scénáře.
/// Sealed record potomci — nelze omylem vytvořit hodnotu s duálním typem.
/// Fáze 1: pouze skalární C# hodnoty.
/// </summary>
public abstract record ContractValue
{
    public required TypeModel Type { get; init; }

    /// <summary>Null hodnota.</summary>
    public sealed record Null : ContractValue
    {
        public Null() : base() => Type = TypeModel.Object;
    }

    /// <summary>String hodnota.</summary>
    public sealed record String(string Value) : ContractValue
    {
        public String() : this("") { }
        public string Value { get; } = Value;
    }

    /// <summary>Int32 hodnota.</summary>
    public sealed record Int32(int Value) : ContractValue;
    public sealed record Decimal(decimal Value) : ContractValue;
    public sealed record Boolean(bool Value) : ContractValue;

    /// <summary>Guid hodnota.</summary>
    public sealed record SystemGuid(Guid Value) : ContractValue;

    /// <summary>DateTimeOffset hodnota.</summary>
    public sealed record DateTimeOffsetValue(System.DateTimeOffset Value) : ContractValue;

    /// <summary>Enum hodnota — jméno typu + string hodnota.</summary>
    public sealed record EnumValue(string TypeName, string Value) : ContractValue;

    /// <summary>StrongType reference — ID + serializovaná hodnota.</summary>
    public sealed record StrongTypeValue(string TypeReferenceId, string SerializedValue) : ContractValue;
}
