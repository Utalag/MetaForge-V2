namespace MetaForge.BusinessModel;

public sealed class BusinessDocumentStore
{
    private readonly BusinessDocumentValidator _validator = new();
    private readonly BusinessIdAllocator _idAllocator = new();

    public BusinessDocumentStore(string documentPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentPath);
        DocumentPath = Path.GetFullPath(documentPath);
    }

    public string DocumentPath { get; }

    public bool Exists => File.Exists(DocumentPath);

    public BusinessAuthoringDocument CreateEmpty(string projectName = "NewProject")
    {
        return new BusinessAuthoringDocument
        {
            Project = new BusinessProjectInfo
            {
                Id = _idAllocator.CreateProjectId(projectName),
                Name = string.IsNullOrWhiteSpace(projectName) ? "NewProject" : projectName,
                Version = 1,
            },
        };
    }

    public BusinessAuthoringDocument Load()
    {
        var json = File.ReadAllText(DocumentPath);
        var document = BusinessDocumentJsonSerializer.Parse(json);
        _validator.EnsureValid(document);
        return document;
    }

    public BusinessAuthoringDocument? TryLoad()
    {
        return Exists ? Load() : null;
    }

    public string ReadJson()
    {
        return Exists ? File.ReadAllText(DocumentPath) : string.Empty;
    }

    public string Serialize(BusinessAuthoringDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        _validator.EnsureValid(document);
        return BusinessDocumentJsonSerializer.Serialize(document);
    }

    public BusinessAuthoringDocument Save(BusinessAuthoringDocument document, bool reloadFromDisk = true)
    {
        ArgumentNullException.ThrowIfNull(document);

        var json = Serialize(document);
        var directory = Path.GetDirectoryName(DocumentPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(DocumentPath, json);
        return reloadFromDisk ? Load() : BusinessDocumentJsonSerializer.Parse(json);
    }
}