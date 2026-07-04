using MetaForge.BusinessModel.CommandLog;

namespace MetaForge.BusinessModel.Migration;

/// <summary>
/// Rozhraní pro migraci commandů mezi verzemi schématu.
/// Každá implementace řeší migraci mezi dvěma konkrétními verzemi.
/// </summary>
public interface ICommandMigration
{
    /// <summary>Verze schématu, ze které migrujeme.</summary>
    string FromVersion { get; }

    /// <summary>Verze schématu, do které migrujeme.</summary>
    string ToVersion { get; }

    /// <summary>Migruje jeden command na novou verzi.</summary>
    CommandEnvelope Migrate(CommandEnvelope command);
}

/// <summary>
/// Engine pro správu a spouštění migrací commandů.
/// Řetězí migrace podle verzí schématu.
/// </summary>
public sealed class CommandMigrationEngine
{
    private readonly List<ICommandMigration> _migrations = new();

    /// <summary>Zaregistruje migraci mezi dvěma verzemi.</summary>
    public void RegisterMigration(ICommandMigration migration)
    {
        _migrations.Add(migration);
    }

    /// <summary>
    /// Migruje command na nejnovější verzi — prochází všechny registrované migrace.
    /// </summary>
    public CommandEnvelope Migrate(CommandEnvelope command)
    {
        var current = command;
        foreach (var migration in _migrations)
        {
            if (current.SchemaVersion == migration.FromVersion)
                current = migration.Migrate(current);
        }
        return current;
    }

    /// <summary>Migruje celou sekvenci commandů.</summary>
    public IReadOnlyList<CommandEnvelope> MigrateAll(IReadOnlyList<CommandEnvelope> commands)
        => commands.Select(Migrate).ToList().AsReadOnly();
}

/// <summary>
/// Vzorová migrace V1.0 → V2.0 — ukázka jak migrovat payload z plain string na JSON.
/// </summary>
public sealed class V1ToV2Migration : ICommandMigration
{
    public string FromVersion => "1.0";
    public string ToVersion => "2.0";

    public CommandEnvelope Migrate(CommandEnvelope command) => command.CommandType switch
    {
        "AddEntity" => command with
        {
            Payload = $$"""{"name": "{{command.Payload}}"}""",
            SchemaVersion = ToVersion
        },
        _ => command with { SchemaVersion = ToVersion }
    };
}
