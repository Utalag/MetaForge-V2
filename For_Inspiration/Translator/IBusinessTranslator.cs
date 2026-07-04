using MetaForge.BusinessModel;
using MetaForge.Core.Common;
using MetaForge.Dto;

namespace MetaForge.Translator;

/// <summary>
/// Translates a <see cref="BusinessAuthoringDocument"/> into a <see cref="MetaForgeTransportDto"/>.
/// </summary>
public interface IBusinessTranslator
{
    MetaForgeTransportDto Translate(BusinessAuthoringDocument document, ProgramLanguage language = ProgramLanguage.CSharp);
}
