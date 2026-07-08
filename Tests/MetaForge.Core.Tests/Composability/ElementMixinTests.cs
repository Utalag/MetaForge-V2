// ---------------------------------------------------------------------------
// MetaForge.Core.Tests — ElementMixinTests
// PROPOSAL: PROP-039 — Core Composability
// ---------------------------------------------------------------------------

using MetaForge.Core.Composability;
using MetaForge.Core.Elements.Members;

namespace MetaForge.Core.Tests.Composability;

public class ElementMixinTests
{
    [Fact]
    public void AuditableMixin_HasCorrectProperties()
    {
        var mixin = BuiltInMixins.Auditable;

        Assert.Equal("Auditable", mixin.Name);
        Assert.Equal(3, mixin.Properties.Count);
        Assert.Contains(mixin.Properties, p => p.Name == "CreatedAt");
        Assert.Contains(mixin.Properties, p => p.Name == "UpdatedAt");
        Assert.Contains(mixin.Properties, p => p.Name == "CreatedBy");
        Assert.Equal(ConflictStrategy.Throw, mixin.OnConflict);
    }

    [Fact]
    public void SoftDeleteMixin_HasPropertiesAndMethods()
    {
        var mixin = BuiltInMixins.SoftDelete;

        Assert.Equal("SoftDelete", mixin.Name);
        Assert.Equal(2, mixin.Properties.Count);
        Assert.Equal(2, mixin.Methods.Count);
        Assert.Contains(mixin.Properties, p => p.Name == "IsDeleted");
        Assert.Contains(mixin.Properties, p => p.Name == "DeletedAt");
        Assert.Contains(mixin.Methods, m => m.Name == "SoftDelete");
        Assert.Contains(mixin.Methods, m => m.Name == "Restore");
    }

    [Fact]
    public void ConflictStrategy_Enum_HasExpectedValues()
    {
        Assert.Equal(0, (int)ConflictStrategy.Skip);
        Assert.Equal(1, (int)ConflictStrategy.Throw);
        Assert.Equal(2, (int)ConflictStrategy.Replace);
    }

    [Fact]
    public void BuiltInMixins_All_ContainsBoth()
    {
        var all = BuiltInMixins.All;
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public void ElementMixin_Record_IsImmutable()
    {
        var mixin = BuiltInMixins.Auditable;
        // Records are immutable — with-expression creates a new instance
        var modified = mixin with { OnConflict = ConflictStrategy.Skip };

        Assert.NotSame(mixin, modified);
        Assert.Equal(ConflictStrategy.Throw, mixin.OnConflict);
        Assert.Equal(ConflictStrategy.Skip, modified.OnConflict);
    }

    [Fact]
    public void ElementMixin_Attributes_DefaultsToNull()
    {
        var mixin = new ElementMixin("Test", Array.Empty<PropertyElement>(), Array.Empty<MethodElement>());
        Assert.Null(mixin.Attributes);
    }
}
