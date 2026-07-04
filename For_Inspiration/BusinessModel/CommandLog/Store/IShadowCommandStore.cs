namespace MetaForge.BusinessModel;

public interface IShadowCommandStore
{
    string FilePath { get; }

    ShadowCommandAppendResult Append(CommandEnvelope envelope);
}