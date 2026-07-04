namespace MetaForge.BusinessModel;

public interface IProjectionQueryService
{
    Task<BusinessProjectionView> GetProjectionAsync(string? streamId = null, CancellationToken cancellationToken = default);
}