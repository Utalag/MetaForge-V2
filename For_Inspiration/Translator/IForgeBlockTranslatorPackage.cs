namespace MetaForge.Translator;

public interface IForgeBlockTranslatorPackage
{
    void RegisterTranslator(IForgeBlockTranslatorRegistry registry);
}

public interface IForgeBlockTranslatorRegistry
{
    void Register(IForgeBlockTranslatorPackage package);
}
