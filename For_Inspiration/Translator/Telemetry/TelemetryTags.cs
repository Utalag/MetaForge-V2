namespace MetaForge.Translator.Telemetry;

/// <summary>
/// Povolene nizkokardinalitni tagy pro MetaForge platformni telemetry.
/// </summary>
public static class TelemetryTags
{
    // Tag names
    public const string Host = "host";
    public const string Source = "source";
    public const string Result = "result";
    public const string Detail = "detail";
    public const string Command = "command";
    public const string ExportKind = "export_kind";
    public const string Provider = "provider";

    // Host values
    public const string HostCli = "cli";
    public const string HostChat = "chat";
    public const string HostMcp = "mcp";
    public const string HostDesktop = "desktop";
    public const string HostExamples = "examples";
    public const string HostTests = "tests";

    // Source values
    public const string SourceCli = "cli";
    public const string SourceChat = "chat";
    public const string SourceMcp = "mcp";
    public const string SourceDesktop = "desktop";
    public const string SourceSystem = "system";

    // Result values
    public const string ResultOk = "ok";
    public const string ResultError = "error";
    public const string ResultValidationError = "validation_error";
    public const string ResultCancelled = "cancelled";

    // Detail values
    public const string DetailBasic = "basic";
    public const string DetailExpert = "expert";
    public const string DetailCustom = "custom";
}
