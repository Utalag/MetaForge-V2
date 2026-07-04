namespace MetaForge.BusinessModel;

public sealed class BusinessProjectInfo
{
    public string Id { get; init; } = "new-project";

    public string Name { get; init; } = "NewProject";

    public string? Description { get; init; }

    public string? Icon { get; init; }

    public int Version { get; init; } = 1;
}