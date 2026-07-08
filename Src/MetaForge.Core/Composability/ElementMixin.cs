// ---------------------------------------------------------------------------
// MetaForge.Core — ElementMixin
// Mixin/Trait system for build-time composition of shared behavior.
// Vrstva: Core / Composability
// 
// PROPOSAL: PROP-039 — Core Composability
// ---------------------------------------------------------------------------

using MetaForge.Core.Abstractions;
using MetaForge.Core.Elements.Members;

namespace MetaForge.Core.Composability;

/// <summary>
/// Strategy for resolving conflicts when a mixin tries to add a member
/// that already exists on the target class.
/// </summary>
public enum ConflictStrategy
{
    /// <summary>Skip the conflicting member — keep the existing one.</summary>
    Skip = 0,

    /// <summary>Throw an error — mixing is not allowed when conflicts exist.</summary>
    Throw = 1,

    /// <summary>Replace the existing member with the mixin's version.</summary>
    Replace = 2
}

/// <summary>
/// A reusable set of properties, methods, and attributes that can be
/// applied (build-time expanded) into a ClassElement.
/// 
/// Mixins are not runtime weaving — they are expanded at build/translate time.
/// </summary>
/// <param name="Name">Unique name of the mixin (e.g., "Auditable", "SoftDelete").</param>
/// <param name="Properties">Properties to add to the target class.</param>
/// <param name="Methods">Methods to add to the target class.</param>
/// <param name="Attributes">Optional attributes to apply at the class level.</param>
/// <param name="OnConflict">How to handle member name conflicts with existing members.</param>
public sealed record ElementMixin(
    string Name,
    IReadOnlyList<PropertyElement> Properties,
    IReadOnlyList<MethodElement> Methods,
    IReadOnlyList<AttributeElement>? Attributes = null,
    ConflictStrategy OnConflict = ConflictStrategy.Throw
);

/// <summary>
/// Registry of built-in mixins for common cross-cutting concerns.
/// </summary>
public static class BuiltInMixins
{
    /// <summary>
    /// Auditable mixin — adds CreatedAt, UpdatedAt, CreatedBy properties.
    /// Typical use: entity classes that need audit trails.
    /// </summary>
    public static readonly ElementMixin Auditable = new(
        Name: "Auditable",
        Properties: new[]
        {
            new PropertyElement
            {
                Name = "CreatedAt",
                Type = DataTypes.TypeModel.Of(DataTypes.DataType.DateTimeOffset),
                HasSetter = false,
                IsInitOnly = true
            },
            new PropertyElement
            {
                Name = "UpdatedAt",
                Type = DataTypes.TypeModel.Of(DataTypes.DataType.DateTimeOffset),
                HasGetter = true,
                HasSetter = true
            },
            new PropertyElement
            {
                Name = "CreatedBy",
                Type = DataTypes.TypeModel.String,
                HasSetter = false,
                IsInitOnly = true
            }
        },
        Methods: Array.Empty<MethodElement>(),
        OnConflict: ConflictStrategy.Throw
    );

    /// <summary>
    /// SoftDelete mixin — adds IsDeleted and DeletedAt properties for soft-delete pattern.
    /// </summary>
    public static readonly ElementMixin SoftDelete = new(
        Name: "SoftDelete",
        Properties: new[]
        {
            new PropertyElement
            {
                Name = "IsDeleted",
                Type = DataTypes.TypeModel.Bool,
                AccessModifier = AccessModifier.Private,
                HasGetter = true,
                HasSetter = true
            },
            new PropertyElement
            {
                Name = "DeletedAt",
                Type = DataTypes.TypeModel.Of(DataTypes.DataType.DateTimeOffset).MakeNullable(),
                AccessModifier = AccessModifier.Private,
                HasGetter = true,
                HasSetter = true
            }
        },
        Methods: new[]
        {
            new MethodElement
            {
                Name = "SoftDelete",
                ReturnType = DataTypes.TypeModel.Void,
                AccessModifier = AccessModifier.Public
            },
            new MethodElement
            {
                Name = "Restore",
                ReturnType = DataTypes.TypeModel.Void,
                AccessModifier = AccessModifier.Public
            }
        },
        OnConflict: ConflictStrategy.Throw
    );

    /// <summary>All built-in mixins.</summary>
    public static IReadOnlyList<ElementMixin> All { get; } = new[] { Auditable, SoftDelete };
}
