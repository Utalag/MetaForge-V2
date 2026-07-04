namespace MetaForge.Infrastructure.FileSystem;

/// <summary>
/// Poskytovatel souborového systému — abstrakce pro testovatelnost.
/// Není sealed, aby bylo možné mockovat v testech.
/// </summary>
public class FileSystemProvider
{
    /// <summary>
    /// Ověří, zda adresář existuje.
    /// </summary>
    public virtual bool DirectoryExists(string path) => Directory.Exists(path);

    /// <summary>
    /// Vytvoří adresář včetně rodičovských.
    /// </summary>
    public virtual void CreateDirectory(string path) => Directory.CreateDirectory(path);

    /// <summary>
    /// Ověří, zda soubor existuje.
    /// </summary>
    public virtual bool FileExists(string path) => File.Exists(path);

    /// <summary>
    /// Přečte celý textový soubor.
    /// </summary>
    public virtual Task<string> ReadAllTextAsync(string path, CancellationToken ct = default)
        => File.ReadAllTextAsync(path, ct);

    /// <summary>
    /// Zapíše celý textový soubor.
    /// </summary>
    public virtual Task WriteAllTextAsync(string path, string content, CancellationToken ct = default)
        => File.WriteAllTextAsync(path, content, ct);

    /// <summary>
    /// Připojí text na konec souboru.
    /// </summary>
    public virtual void AppendAllText(string path, string content)
        => File.AppendAllText(path, content);

    /// <summary>
    /// Přečte všechny řádky souboru.
    /// </summary>
    public virtual Task<string[]> ReadAllLinesAsync(string path, CancellationToken ct = default)
        => File.ReadAllLinesAsync(path, ct);

    /// <summary>
    /// Vrátí soubory odpovídající vzoru v adresáři.
    /// </summary>
    public virtual string[] GetFiles(string path, string searchPattern)
        => Directory.GetFiles(path, searchPattern);
}
