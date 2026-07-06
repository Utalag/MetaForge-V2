using System.Text.Json;
using System.Text.Json.Serialization;

namespace MetaForge.BusinessModel.Models;

/// <summary>
/// Typový stav synchronizace mezi business vrstvou a Core vrstvou.
/// Discriminated union — kompilátor vynutí pokrytí všech přechodů (exhaustive switch).
/// Nahrazuje původní <see cref="AttributeSyncState"/> enum.
/// </summary>
[JsonConverter(typeof(SyncStateJsonConverter))]
public abstract record SyncState
{
    /// <summary>Nově vytvořený atribut — ještě nebyl synchronizován s Core.</summary>
    public sealed record New : SyncState;

    /// <summary>Business a Core jsou synchronizované — žádné změny.</summary>
    /// <param name="SyncedAt">Čas poslední synchronizace.</param>
    public sealed record Synced(DateTimeOffset SyncedAt) : SyncState;

    /// <summary>Business atribut byl upraven — CoreDetail je neaktuální.</summary>
    /// <param name="Previous">Předchozí stav (obvykle Synced).</param>
    public sealed record BusinessEdited(SyncState Previous) : SyncState;

    /// <summary>CoreDetail byl upraven (např. AI enrichment) — business atribut je neaktuální.</summary>
    /// <param name="Previous">Předchozí stav (obvykle Synced).</param>
    public sealed record CoreEdited(SyncState Previous) : SyncState;

    /// <summary>Business i Core byly upraveny nezávisle — vzniknul konflikt.</summary>
    /// <param name="Reason">Důvod konfliktu.</param>
    /// <param name="Business">Stav business vrstvy.</param>
    /// <param name="Core">Stav Core vrstvy.</param>
    public sealed record Conflict(string Reason, SyncState Business, SyncState Core) : SyncState;

    /// <summary>Přechodová funkce — uživatel upravil business atribut.</summary>
    public SyncState OnBusinessEdit() => this switch
    {
        Synced s => new BusinessEdited(s),
        CoreEdited c => new Conflict("both edited", new BusinessEdited(c.Previous), c),
        BusinessEdited => this,       // již označeno — idempotentní
        Conflict => this,             // již v konfliktu
        New => this,                  // nový atribut — zatím žádná synchronizace
        _ => this,                    // budoucí rozšíření
    };

    /// <summary>Přechodová funkce — Core vrstva upravila CoreDetail (např. AI enrichment).</summary>
    public SyncState OnCoreEdit() => this switch
    {
        Synced s => new CoreEdited(s),
        BusinessEdited b => new Conflict("both edited", b, new CoreEdited(b.Previous)),
        CoreEdited => this,           // již označeno — idempotentní
        Conflict => this,             // již v konfliktu
        New => this,                  // nový atribut — zatím žádná synchronizace
        _ => this,                    // budoucí rozšíření
    };

    /// <summary>Přechodová funkce — synchronizace proběhla úspěšně.</summary>
    public SyncState OnSynced() => new Synced(DateTimeOffset.UtcNow);

    /// <summary>
    /// Převede starý <see cref="AttributeSyncState"/> enum na nový typový SyncState.
    /// Slouží pro migraci existujících command logů.
    /// </summary>
    public static SyncState FromLegacyEnum(AttributeSyncState legacy)
    {
        return legacy switch
        {
            AttributeSyncState.New => new New(),
            AttributeSyncState.Synced => new Synced(DateTimeOffset.UtcNow),
            AttributeSyncState.BusinessEdited => new BusinessEdited(new Synced(DateTimeOffset.UtcNow)),
            AttributeSyncState.CoreEdited => new CoreEdited(new Synced(DateTimeOffset.UtcNow)),
            AttributeSyncState.Conflict => new Conflict("historický konflikt", new BusinessEdited(new Synced(DateTimeOffset.UtcNow)), new CoreEdited(new Synced(DateTimeOffset.UtcNow))),
            _ => new New(),
        };
    }
}

/// <summary>
/// JSON konvertor pro polymorfní serializaci/deserializaci <see cref="SyncState"/>.
/// Používá diskriminační pole "$type".
/// </summary>
public sealed class SyncStateJsonConverter : JsonConverter<SyncState>
{
    private const string TypeField = "$type";

    public override SyncState? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            // Zkusit parsovat jako starý enum string (migrační fallback)
            if (reader.TokenType == JsonTokenType.String)
            {
                var enumStr = reader.GetString();
                if (Enum.TryParse<AttributeSyncState>(enumStr, out var legacy))
                    return SyncState.FromLegacyEnum(legacy);
            }
            return null;
        }

        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty(TypeField, out var typeProp))
            return new SyncState.New(); // fallback

        var typeName = typeProp.GetString();
        return typeName switch
        {
            nameof(SyncState.New) => new SyncState.New(),
            nameof(SyncState.Synced) => new SyncState.Synced(
                root.TryGetProperty("syncedAt", out var sa) && sa.TryGetDateTimeOffset(out var dt) ? dt : DateTimeOffset.UtcNow),
            nameof(SyncState.BusinessEdited) => new SyncState.BusinessEdited(
                root.TryGetProperty("previous", out var bp) ? JsonSerializer.Deserialize<SyncState>(bp.GetRawText(), options) ?? new SyncState.New() : new SyncState.New()),
            nameof(SyncState.CoreEdited) => new SyncState.CoreEdited(
                root.TryGetProperty("previous", out var cp) ? JsonSerializer.Deserialize<SyncState>(cp.GetRawText(), options) ?? new SyncState.New() : new SyncState.New()),
            nameof(SyncState.Conflict) => new SyncState.Conflict(
                root.TryGetProperty("reason", out var r) ? r.GetString() ?? "neznámý" : "neznámý",
                root.TryGetProperty("business", out var b) ? JsonSerializer.Deserialize<SyncState>(b.GetRawText(), options) ?? new SyncState.New() : new SyncState.New(),
                root.TryGetProperty("core", out var c) ? JsonSerializer.Deserialize<SyncState>(c.GetRawText(), options) ?? new SyncState.New() : new SyncState.New()),
            _ => new SyncState.New(),
        };
    }

    public override void Write(Utf8JsonWriter writer, SyncState value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(TypeField, value.GetType().Name);

        switch (value)
        {
            case SyncState.Synced s:
                writer.WriteString("syncedAt", s.SyncedAt);
                break;
            case SyncState.BusinessEdited be:
                writer.WritePropertyName("previous");
                JsonSerializer.Serialize(writer, be.Previous, options);
                break;
            case SyncState.CoreEdited ce:
                writer.WritePropertyName("previous");
                JsonSerializer.Serialize(writer, ce.Previous, options);
                break;
            case SyncState.Conflict cf:
                writer.WriteString("reason", cf.Reason);
                writer.WritePropertyName("business");
                JsonSerializer.Serialize(writer, cf.Business, options);
                writer.WritePropertyName("core");
                JsonSerializer.Serialize(writer, cf.Core, options);
                break;
            // New nemá žádná data
        }

        writer.WriteEndObject();
    }
}
