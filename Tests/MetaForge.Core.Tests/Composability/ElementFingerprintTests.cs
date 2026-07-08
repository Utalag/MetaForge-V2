// ---------------------------------------------------------------------------
// MetaForge.Core.Tests — ElementFingerprintTests
// PROPOSAL: PROP-039 — Core Composability
// ---------------------------------------------------------------------------

using MetaForge.Core.Composability;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Types;

namespace MetaForge.Core.Tests.Composability;

public class ElementFingerprintTests
{
    [Fact]
    public void Compute_FromParts_ProducesConsistentHash()
    {
        var fp1 = ElementFingerprint.Compute(new[] { "class:Foo", "ns:Test" }, 1);
        var fp2 = ElementFingerprint.Compute(new[] { "class:Foo", "ns:Test" }, 1);

        Assert.Equal(fp1.StructuralHash, fp2.StructuralHash);
        Assert.Equal(fp1, fp2);
    }

    [Fact]
    public void Compute_DifferentContent_ProducesDifferentHash()
    {
        var fp1 = ElementFingerprint.Compute("class:Foo", 1);
        var fp2 = ElementFingerprint.Compute("class:Bar", 1);

        Assert.NotEqual(fp1, fp2);
    }

    [Fact]
    public void Compute_DifferentPipelineVersion_ProducesDifferentFingerprint()
    {
        var fp1 = ElementFingerprint.Compute("class:Foo", 1);
        var fp2 = ElementFingerprint.Compute("class:Foo", 2);

        Assert.NotEqual(fp1, fp2);
    }

    [Fact]
    public void Empty_HasZeroHash()
    {
        var empty = ElementFingerprint.Empty;
        Assert.StartsWith("00000000", empty.StructuralHash);
        Assert.Equal(0, empty.PipelineVersion);
    }

    [Fact]
    public void ClassElementFingerprint_ProducesConsistentResult()
    {
        var cls = new ClassElement
        {
            Name = "Customer",
            Namespace = "MyApp.Models",
            IsSealed = true,
            Properties =
            {
                new PropertyElement { Name = "Id", Type = TypeModel.Int32 }
            }
        };

        var fp1 = cls.ComputeFingerprint(1);
        var fp2 = cls.ComputeFingerprint(1);

        Assert.Equal(fp1, fp2);
    }

    [Fact]
    public void ClassElementFingerprint_DetectsChanges()
    {
        var cls1 = new ClassElement { Name = "Customer", IsSealed = false };
        var cls2 = new ClassElement { Name = "Customer", IsSealed = true };

        var fp1 = cls1.ComputeFingerprint(1);
        var fp2 = cls2.ComputeFingerprint(1);

        Assert.NotEqual(fp1, fp2);
    }

    [Fact]
    public void MethodElementFingerprint_ProducesConsistentResult()
    {
        var method = new MethodElement
        {
            Name = "GetById",
            ReturnType = TypeModel.String,
            IsAsync = true
        };

        var fp1 = method.ComputeFingerprint(1);
        var fp2 = method.ComputeFingerprint(1);

        Assert.Equal(fp1, fp2);
    }

    [Fact]
    public void ToString_ShowsShortHash()
    {
        var fp = ElementFingerprint.Compute("test", 3);
        var str = fp.ToString();

        Assert.Contains("@v3", str);
        Assert.Contains("...", str);
    }

    [Fact]
    public void Equality_Operators()
    {
        var fp1 = ElementFingerprint.Compute("a", 1);
        var fp2 = ElementFingerprint.Compute("a", 1);
        var fp3 = ElementFingerprint.Compute("b", 1);

        Assert.True(fp1 == fp2);
        Assert.True(fp1 != fp3);
    }

    [Fact]
    public void GetHashCode_Equal_Fingerprints_HaveEqualHashCodes()
    {
        var fp1 = ElementFingerprint.Compute("test", 42);
        var fp2 = ElementFingerprint.Compute("test", 42);

        Assert.Equal(fp1.GetHashCode(), fp2.GetHashCode());
    }
}
