// ---------------------------------------------------------------------------
// MetaForge.Core.Tests — ConventionRegistryTests
// PROPOSAL: PROP-039 — Core Composability
// ---------------------------------------------------------------------------

using MetaForge.Core.Abstractions;
using MetaForge.Core.Composability;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Types;

namespace MetaForge.Core.Tests.Composability;

public class ConventionRegistryTests
{
    [Fact]
    public void Registry_Register_AddsConvention()
    {
        var registry = new ConventionRegistry();
        var convention = new PascalCasePropertiesConvention();

        registry.Register(convention);

        Assert.Single(registry.Conventions);
        Assert.Same(convention, registry.Conventions[0]);
    }

    [Fact]
    public void Registry_RegisterRange_AddsMultiple()
    {
        var registry = new ConventionRegistry();
        registry.RegisterRange(new IConvention[]
        {
            new PascalCasePropertiesConvention(),
            new InterfacePrefixConvention(),
            new AsyncSuffixConvention()
        });

        Assert.Equal(3, registry.Conventions.Count);
    }

    [Fact]
    public void PascalCaseConvention_Diagnoses_LowercaseProperty()
    {
        var convention = new PascalCasePropertiesConvention();
        var context = new ConventionContext();

        var cls = new ClassElement
        {
            Name = "TestClass",
            Properties =
            {
                new PropertyElement
                {
                    Name = "myProperty", // lowercase — should trigger
                    Type = TypeModel.String
                }
            }
        };

        Assert.True(convention.AppliesTo(cls));
        convention.Apply(cls, context);

        var diagnostics = context.Diagnostics.ToReadOnly();
        Assert.Single(diagnostics);
        Assert.Equal("MF-CONV-001", diagnostics[0].Code);
        Assert.Contains("myProperty", diagnostics[0].Message);
    }

    [Fact]
    public void PascalCaseConvention_SkipsPascalCaseProperty()
    {
        var convention = new PascalCasePropertiesConvention();
        var context = new ConventionContext();

        var cls = new ClassElement
        {
            Name = "TestClass",
            Properties =
            {
                new PropertyElement
                {
                    Name = "MyProperty", // PascalCase — should NOT trigger
                    Type = TypeModel.String
                }
            }
        };

        convention.Apply(cls, context);
        Assert.Empty(context.Diagnostics.ToReadOnly());
    }

    [Fact]
    public void InterfacePrefixConvention_Diagnoses_MissingPrefix()
    {
        var convention = new InterfacePrefixConvention();
        var context = new ConventionContext();

        var iface = new InterfaceElement { Name = "Repository" }; // missing I prefix

        Assert.True(convention.AppliesTo(iface));
        convention.Apply(iface, context);

        var diagnostics = context.Diagnostics.ToReadOnly();
        Assert.Single(diagnostics);
        Assert.Equal("MF-CONV-002", diagnostics[0].Code);
    }

    [Fact]
    public void InterfacePrefixConvention_Skips_ValidPrefix()
    {
        var convention = new InterfacePrefixConvention();
        var context = new ConventionContext();

        var iface = new InterfaceElement { Name = "IRepository" };

        convention.Apply(iface, context);
        Assert.Empty(context.Diagnostics.ToReadOnly());
    }

    [Fact]
    public void AsyncSuffixConvention_Diagnoses_MissingSuffix()
    {
        var convention = new AsyncSuffixConvention();
        var context = new ConventionContext();

        var cls = new ClassElement
        {
            Name = "Service",
            Methods =
            {
                new MethodElement
                {
                    Name = "FetchData", // missing Async suffix
                    IsAsync = true
                }
            }
        };

        Assert.True(convention.AppliesTo(cls));
        convention.Apply(cls, context);

        var diagnostics = context.Diagnostics.ToReadOnly();
        Assert.Single(diagnostics);
        Assert.Equal("MF-CONV-003", diagnostics[0].Code);
    }

    [Fact]
    public void AsyncSuffixConvention_Skips_ValidSuffix()
    {
        var convention = new AsyncSuffixConvention();
        var context = new ConventionContext();

        var cls = new ClassElement
        {
            Name = "Service",
            Methods =
            {
                new MethodElement
                {
                    Name = "FetchDataAsync",
                    IsAsync = true
                }
            }
        };

        convention.Apply(cls, context);
        Assert.Empty(context.Diagnostics.ToReadOnly());
    }

    [Fact]
    public void Registry_ApplyTo_AppliesAllConventions()
    {
        var registry = new ConventionRegistry();
        registry.Register(new InterfacePrefixConvention());

        IReadOnlyList<RootElement> elements = new RootElement[]
        {
            new InterfaceElement { Name = "Repository" },
            new InterfaceElement { Name = "IService" }
        };

        var context = new ConventionContext();
        var result = registry.ApplyTo(elements, context);

        Assert.Equal(2, result.Count);
        var diagnostics = context.Diagnostics.ToReadOnly();
        Assert.Single(diagnostics); // Only "Repository" triggers
    }

    [Fact]
    public void ConventionDoesNotApply_ToWrongElementType()
    {
        var convention = new InterfacePrefixConvention();
        var cls = new ClassElement { Name = "Customer" };

        Assert.False(convention.AppliesTo(cls));
    }
}
