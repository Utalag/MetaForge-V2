namespace MetaForge.BusinessModel.CommandLog;

/// <summary>
/// Určuje, odkud command přišel — jaké host surface ho vytvořilo.
/// </summary>
public enum CommandSource
{
    /// <summary>Původ není znám.</summary>
    Unknown = 0,

    /// <summary>Z chatovacího rozhraní (CLI chat, MCP chat).</summary>
    Chat = 1,

    /// <summary>Z CLI příkazové řádky.</summary>
    Cli = 2,

    /// <summary>Z MCP (Model Context Protocol) klienta.</summary>
    Mcp = 3,

    /// <summary>Importováno z externího souboru/formátu.</summary>
    Import = 4,

    /// <summary>Vygenerováno systémem (např. auto-save, migrace).</summary>
    System = 5,

    /// <summary>Z WebApi REST rozhraní.</summary>
    WebApi = 6,

    /// <summary>Z desktopové aplikace.</summary>
    Desktop = 7,
}
