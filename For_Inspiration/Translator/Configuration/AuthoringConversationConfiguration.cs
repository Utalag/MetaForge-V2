using System.Text.Json;
using System.Text.Json.Serialization;
using MetaForge.BusinessModel;

namespace MetaForge.Translator;

public sealed class AuthoringConversationConfiguration
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    [JsonIgnore]
    public string ConfigurationFilePath { get; private set; } = string.Empty;

    public AuthoringPersistenceSettings Persistence { get; set; } = new();

    public AuthoringTreeSettings Tree { get; set; } = new();

    public AuthoringPromptingSettings Prompting { get; set; } = new();

    public AuthoringShadowLogSettings ShadowLog { get; set; } = new();

    public AuthoringEnrichmentSettings Enrichment { get; set; } = new();

    /// <summary>
    /// Načte konfiguraci z JSON souboru. Pokud cesta není zadána, hledá se výchozí soubor.
    /// Pokud soubor neexistuje, vrátí výchozí konfiguraci.
    /// </summary>
    public static AuthoringConversationConfiguration Load(string? filePath = null)
    {
        var resolvedPath = ResolveConfigurationPath(filePath);
        AuthoringConversationConfiguration configuration;

        if (File.Exists(resolvedPath))
        {
            var json = File.ReadAllText(resolvedPath);
            configuration = JsonSerializer.Deserialize<AuthoringConversationConfiguration>(json, SerializerOptions) ?? new AuthoringConversationConfiguration();
        }
        else

        {
            configuration = new AuthoringConversationConfiguration();
        }

        configuration.ConfigurationFilePath = resolvedPath;
        configuration.Normalize();
        return configuration;
    }

    /// <summary>
    /// Uloží aktuální konfiguraci do JSON souboru. Pokud není cesta zadána, použije se naposledy použitá cesta.
    /// Vytvoří adresář, pokud neexistuje.
    /// </summary>
    public void Save(string? filePath = null)
    {
        var resolvedPath = ResolveConfigurationPath(filePath ?? ConfigurationFilePath);
        ConfigurationFilePath = resolvedPath;
        Normalize();

        var directory = Path.GetDirectoryName(resolvedPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(this, SerializerOptions);
        File.WriteAllText(resolvedPath, json);
    }

    /// <summary>
    /// Vrátí absolutní cestu k dokumentu na základě relativní cesty v Persistence.DocumentPath.
    /// </summary>
    public string GetResolvedDocumentPath()
    {
        return ResolveRelativePath(Persistence.DocumentPath, ConfigurationFilePath);
    }

    /// <summary>
    /// Vrátí absolutní cestu k souboru shadow logu na základě relativní cesty v ShadowLog.Path.
    /// </summary>
    public string GetResolvedShadowLogPath()
    {
        return ResolveRelativePath(ShadowLog.Path, ConfigurationFilePath);
    }

    /// <summary>
    /// Normalizuje výchozí hodnoty nastavení, pokud nejsou explicitně nastaveny.
    /// Nastaví výchozí cesty pro dokument a shadow log, pokud jsou prázdné.
    /// </summary>
    private void Normalize()
    {
        if (string.IsNullOrWhiteSpace(Persistence.DocumentPath))
            Persistence.DocumentPath = Path.Combine("artifacts", "business-authoring-document.json");

        if (string.IsNullOrWhiteSpace(ShadowLog.Path))
            ShadowLog.Path = Path.Combine("artifacts", "business-authoring-command-log.jsonl");
    }

    /// <summary>
    /// Vyhledá cestu ke konfiguračnímu souboru. Pokud je cesta zadána, vrátí její absolutní podobu.
    /// Jinak prohledává adresářovou strukturu směrem nahoru, dokud nenajde soubor metaforge.authoring.json.
    /// Pokud žádný nenajde, vrátí cestu do aktuálního adresáře.
    /// </summary>
    private static string ResolveConfigurationPath(string? filePath)
    {
        if (!string.IsNullOrWhiteSpace(filePath))
            return Path.GetFullPath(filePath);

        const string defaultFileName = "metaforge.authoring.json";
        var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (currentDirectory is not null)
        {
            var candidate = Path.Combine(currentDirectory.FullName, defaultFileName);
            if (File.Exists(candidate))
                return candidate;

            currentDirectory = currentDirectory.Parent;
        }

        return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), defaultFileName));
    }

    /// <summary>
    /// Převádí relativní cestu na absolutní na základě umístění konfiguračního souboru.
    /// Pokud je cesta již absolutní, vrátí ji beze změny.
    /// </summary>
    private static string ResolveRelativePath(string path, string configurationFilePath)
    {
        if (Path.IsPathRooted(path))
            return path;

        var baseDirectory = !string.IsNullOrWhiteSpace(configurationFilePath)
            ? Path.GetDirectoryName(configurationFilePath)
            : Directory.GetCurrentDirectory();

        return Path.GetFullPath(Path.Combine(baseDirectory ?? Directory.GetCurrentDirectory(), path));
    }
}

public sealed class AuthoringPersistenceSettings
{
    public bool Enabled { get; set; } = true;

    public string DocumentPath { get; set; } = Path.Combine("artifacts", "business-authoring-document.json");

    public bool LoadPersistedDocumentOnStartup { get; set; } = true;

    public bool ReloadPersistedDocumentAfterSave { get; set; } = true;
}

public sealed class AuthoringTreeSettings
{
    public BusinessTreeDetailLevel DefaultDetailLevel { get; set; } = BusinessTreeDetailLevel.Extended;
}

public sealed class AuthoringPromptingSettings
{
    public bool AutoApplyModeApply { get; set; } = true;

    public bool AutoTranslateRecommendedBriefs { get; set; } = false;

    public bool RequireConfirmationForPropose { get; set; } = false;

    public bool RequireConfirmationForApply { get; set; } = false;
}

public sealed class AuthoringShadowLogSettings
{
    public bool Enabled { get; set; } = false;

    public string Path { get; set; } = System.IO.Path.Combine("artifacts", "business-authoring-command-log.jsonl");
}

public sealed class AuthoringEnrichmentSettings
{
    public bool AutoEnrichOnAdd { get; set; } = false;

    public bool AutoApplyPresets { get; set; } = false;

    public bool IncludeCoreDetailInTree { get; set; } = true;

    public CoreInfoSource DefaultSource { get; set; } = CoreInfoSource.Generated;
}