using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Types;

namespace MetaForge.Core.Builders;

/// <summary>
/// Fluent builder pro EnumElement.
/// </summary>
public sealed class EnumBuilder
{
    private readonly EnumElement _enum = new();

    public EnumBuilder(string name)
    {
        _enum.Name = name;
    }

    public EnumBuilder Flags() { _enum.IsFlags = true; return this; }
    public EnumBuilder UnderlyingType(DataType dataType) { _enum.UnderlyingType = dataType; return this; }

    public EnumBuilder Member(string name, object? value = null)
    {
        _enum.Members.Add(new EnumMemberElement { Name = name, Value = value });
        return this;
    }

    public EnumBuilder Members(params (string Name, object? Value)[] members)
    {
        foreach (var (name, value) in members)
            Member(name, value);
        return this;
    }

    public EnumBuilder Metadata(Action<MetadataBag> configure)
    {
        configure(_enum.Metadata);
        return this;
    }

    public EnumElement Build() => _enum;
}
