using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;

namespace MetaForge.Core.Builders;

/// <summary>
/// Fluent builder pro ParameterElement.
/// </summary>
public sealed class ParameterBuilder
{
    private readonly ParameterElement _param;

    public ParameterBuilder(string name, TypeModel type)
    {
        _param = new ParameterElement { Name = name, Type = type };
    }

    public ParameterBuilder Default(string value) { _param.HasDefaultValue = true; _param.DefaultValue = value; return this; }
    public ParameterBuilder Ref() { _param.Modifier = ParameterModifier.Ref; return this; }
    public ParameterBuilder Out() { _param.Modifier = ParameterModifier.Out; return this; }
    public ParameterBuilder In() { _param.Modifier = ParameterModifier.In; return this; }
    public ParameterBuilder Params() { _param.Modifier = ParameterModifier.Params; return this; }

    public ParameterElement Build() => _param;
}
