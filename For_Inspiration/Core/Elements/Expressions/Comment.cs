using MetaForge.Core.Abstractions;

namespace MetaForge.Core.Elements.Expressions;

/// <summary>
/// Comment (komentář) - dokumentační nebo inline komentář.
/// </summary>
public class Comment : RootElement, ILanguageElement
{
    private string _text = string.Empty;
    private CommentType _commentType = CommentType.SingleLine;

    /// <summary>
    /// Text komentáře.
    /// </summary>
    public string Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Typ komentáře.
    /// </summary>
    public CommentType CommentType
    {
        get => _commentType;
        set
        {
            if (_commentType != value)
            {
                _commentType = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Vygeneruje kód komentáře.
    /// </summary>
    public string GenerateCode()
    {
        var renderer = ExpressionRendererRegistry.Get(TargetLanguage);
        if (renderer != null)
            return renderer.RenderComment(this);

        return GenerateCodeFallback();
    }

    private string GenerateCodeFallback()
    {
        return TargetLanguage switch
        {
            Common.ProgramLanguage.CSharp => GenerateCSharpComment(),
            Common.ProgramLanguage.TypeScript => GenerateTypeScriptComment(),
            Common.ProgramLanguage.Python => GeneratePythonComment(),
            Common.ProgramLanguage.Java => GenerateJavaComment(),
            Common.ProgramLanguage.Go => GenerateGoComment(),
            _ => GenerateCSharpComment()
        };
    }

    private string GenerateCSharpComment()
    {
        return CommentType switch
        {
            CommentType.SingleLine => $"// {Text}",
            CommentType.MultiLine => $"/* {Text} */",
            CommentType.Documentation => $"/// <summary>\n/// {Text}\n/// </summary>",
            CommentType.Region => $"#region {Text}",
            CommentType.EndRegion => "#endregion",
            _ => $"// {Text}"
        };
    }

    private string GenerateTypeScriptComment()
    {
        return CommentType switch
        {
            CommentType.SingleLine => $"// {Text}",
            CommentType.MultiLine => $"/* {Text} */",
            CommentType.Documentation => $"/**\n * {Text}\n */",
            _ => $"// {Text}"
        };
    }

    private string GeneratePythonComment()
    {
        return CommentType switch
        {
            CommentType.SingleLine => $"# {Text}",
            CommentType.MultiLine => $"\"\"\"{Text}\"\"\"",
            CommentType.Documentation => $"\"\"\"{Text}\"\"\"",
            _ => $"# {Text}"
        };
    }

    private string GenerateJavaComment()
    {
        return CommentType switch
        {
            CommentType.SingleLine => $"// {Text}",
            CommentType.MultiLine => $"/* {Text} */",
            CommentType.Documentation => $"/**\n * {Text}\n */",
            _ => $"// {Text}"
        };
    }

    private string GenerateGoComment()
    {
        return CommentType switch
        {
            CommentType.SingleLine => $"// {Text}",
            CommentType.MultiLine => $"/* {Text} */",
            CommentType.Documentation => $"// {Text}",
            _ => $"// {Text}"
        };
    }
}

/// <summary>
/// Typ komentáře.
/// </summary>
public enum CommentType
{
    /// <summary>
    /// Jednořádkový komentář (// nebo #).
    /// </summary>
    SingleLine,

    /// <summary>
    /// Víceřádkový komentář (/* */ nebo """).
    /// </summary>
    MultiLine,

    /// <summary>
    /// Dokumentační komentář (/// nebo /** */).
    /// </summary>
    Documentation,

    /// <summary>
    /// Region (pouze C#).
    /// </summary>
    Region,

    /// <summary>
    /// End region (pouze C#).
    /// </summary>
    EndRegion
}
