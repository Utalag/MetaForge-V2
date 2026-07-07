using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;

namespace MetaForge.Core.Elements.Types;

/// <summary>
/// Reprezentuje C# delegate — typově bezpečný ukazatel na metodu.
/// Např. `public delegate void EventHandler(object sender, EventArgs e);`.
/// </summary>
public sealed class DelegateElement : RootElement
{
    public override string Kind => "delegate";

    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;

    /// <summary>Návratový typ delegáta.</summary>
    public TypeModel ReturnType { get; set; } = TypeModel.Void;

    /// <summary>Parametry delegáta.</summary>
    public List<ParameterElement> Parameters { get; } = new();

    /// <summary>Generické typové parametry (např. `T` v `delegate T Factory&lt;T&gt;()`).</summary>
    public List<TypeParameterElement> TypeParameters { get; } = new();

    /// <summary>Vytvoří delegate bez parametrů a s návratovým typem void.</summary>
    public static DelegateElement Basic(string name) => new() { Name = name };

    /// <summary>Nastaví access modifier.</summary>
    public DelegateElement WithAccess(AccessModifier access)
    {
        AccessModifier = access;
        return this;
    }

    /// <summary>Přidá parametr.</summary>
    public DelegateElement WithParameter(ParameterElement parameter)
    {
        Parameters.Add(parameter);
        return this;
    }

    /// <summary>Přidá generický typový parametr.</summary>
    public DelegateElement WithTypeParameter(TypeParameterElement typeParameter)
    {
        TypeParameters.Add(typeParameter);
        return this;
    }
}
