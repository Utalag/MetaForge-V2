using FluentAssertions;
using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Types;
using MetaForge.Generators;

namespace MetaForge.Generators.Tests;

/// <summary>
/// Testy pro diagnostiku MapType — PROP-061 Fáze 0 (CODE-004).
/// Ověřuje, že generátor:
/// - Správně mapuje Array (T[]) a Nullable (T?)
/// - Negeneruje TODO komentáře do výstupu
/// - Emituje diagnostiku pro nerozpoznané typy
/// </summary>
public class MapTypeDiagnosticsTests
{
    private readonly CodeGenerator _generator = new();

    // === Array ===

    [Fact]
    public void Generate_ArrayProperty_ProducesBracketSyntax()
    {
        var cls = new ClassElement { Name = "Order" };
        cls.Properties.Add(new PropertyElement
        {
            Name = "Items",
            Type = new TypeModel
            {
                BaseType = DataType.Array,
                GenericArguments = { TypeModel.Int32 }
            }
        });

        var result = _generator.Generate(cls);

        result.SourceCode.Should().Contain("int[] Items");
        result.SourceCode.Should().NotContain("object[]");
    }

    [Fact]
    public void Generate_ArrayProperty_WithoutGeneric_ProducesObjectArray()
    {
        var cls = new ClassElement { Name = "Order" };
        cls.Properties.Add(new PropertyElement
        {
            Name = "Data",
            Type = new TypeModel
            {
                BaseType = DataType.Array,
                GenericArguments = { }
            }
        });

        var result = _generator.Generate(cls);

        result.SourceCode.Should().Contain("object[]");
    }

    // === Nullable ===

    [Fact]
    public void Generate_NullableProperty_ProducesQuestionMarkSyntax()
    {
        var cls = new ClassElement { Name = "Order" };
        cls.Properties.Add(new PropertyElement
        {
            Name = "OptionalId",
            Type = new TypeModel
            {
                BaseType = DataType.Nullable,
                GenericArguments = { TypeModel.Int32 }
            }
        });

        var result = _generator.Generate(cls);

        result.SourceCode.Should().Contain("int? OptionalId");
    }

    [Fact]
    public void Generate_NullableProperty_WithoutGeneric_ProducesObjectNullable()
    {
        var cls = new ClassElement { Name = "Order" };
        cls.Properties.Add(new PropertyElement
        {
            Name = "Data",
            Type = new TypeModel
            {
                BaseType = DataType.Nullable,
                GenericArguments = { }
            }
        });

        var result = _generator.Generate(cls);

        result.SourceCode.Should().Contain("object? Data");
    }

    // === Clean output (no TODO) ===

    [Fact]
    public void Generate_EntityWithoutCustomTypeName_ProducesCleanObject()
    {
        var cls = new ClassElement { Name = "Order" };
        cls.Properties.Add(new PropertyElement
        {
            Name = "Customer",
            Type = new TypeModel { BaseType = DataType.Entity }
        });

        var result = _generator.Generate(cls);

        result.SourceCode.Should().Contain("object Customer");
        result.SourceCode.Should().NotContain("/* TODO");
    }

    [Fact]
    public void Generate_StructWithoutCustomTypeName_ProducesCleanObject()
    {
        var cls = new ClassElement { Name = "Order" };
        cls.Properties.Add(new PropertyElement
        {
            Name = "Coordinates",
            Type = new TypeModel { BaseType = DataType.Struct }
        });

        var result = _generator.Generate(cls);

        result.SourceCode.Should().Contain("object Coordinates");
        result.SourceCode.Should().NotContain("/* Resolved via");
        result.SourceCode.Should().NotContain("/* TODO");
    }

    [Fact]
    public void Generate_RecordWithoutCustomTypeName_ProducesCleanObject()
    {
        var cls = new ClassElement { Name = "Order" };
        cls.Properties.Add(new PropertyElement
        {
            Name = "Snapshot",
            Type = new TypeModel { BaseType = DataType.Record }
        });

        var result = _generator.Generate(cls);

        result.SourceCode.Should().Contain("object Snapshot");
        result.SourceCode.Should().NotContain("/* Resolved via");
        result.SourceCode.Should().NotContain("/* TODO");
    }

    // === Diagnostics ===

    [Fact]
    public void Generate_UnresolvedEntity_EmitsDiagnosticWarning()
    {
        var cls = new ClassElement { Name = "Order" };
        cls.Properties.Add(new PropertyElement
        {
            Name = "Customer",
            Type = new TypeModel { BaseType = DataType.Entity }
        });

        var result = _generator.Generate(cls);

        result.Diagnostics.Should().NotBeNull();
        result.Diagnostics.Should().Contain(d =>
            d.Message.Contains("Nelze namapovat typ") &&
            d.Severity == DiagnosticSeverity.Warning);
    }

    [Fact]
    public void Generate_ResolvedEntity_NoDiagnostic()
    {
        var cls = new ClassElement { Name = "Order" };
        cls.Properties.Add(new PropertyElement
        {
            Name = "Customer",
            Type = new TypeModel
            {
                BaseType = DataType.Entity,
                CustomTypeName = "Customer"
            }
        });

        var result = _generator.Generate(cls);

        // Resolved typ => žádná diagnostika z MapType
        // (mohou existovat jiné, ale ne o nerozpoznaném typu)
        if (result.Diagnostics != null)
        {
            result.Diagnostics.Should().NotContain(d =>
                d.Message.Contains("Nelze namapovat typ"));
        }
    }

    [Fact]
    public void Generate_ResolvedStruct_NoDiagnostic()
    {
        var cls = new ClassElement { Name = "Order" };
        cls.Properties.Add(new PropertyElement
        {
            Name = "Coordinates",
            Type = new TypeModel
            {
                BaseType = DataType.Struct,
                CustomTypeName = "Point"
            }
        });

        var result = _generator.Generate(cls);

        if (result.Diagnostics != null)
        {
            result.Diagnostics.Should().NotContain(d =>
                d.Message.Contains("Nelze namapovat typ"));
        }
    }

    // === IsCollection unchanged ===

    [Fact]
    public void Generate_CollectionProperty_StillProducesList()
    {
        var cls = new ClassElement { Name = "Order" };
        cls.Properties.Add(new PropertyElement
        {
            Name = "Tags",
            Type = new TypeModel
            {
                IsCollection = true,
                GenericArguments = { TypeModel.String }
            }
        });

        var result = _generator.Generate(cls);

        result.SourceCode.Should().Contain("List<string>");
    }

    // === No TODO in any output ===

    [Fact]
    public void Generate_AllPrimitiveTypes_NoTodoInOutput()
    {
        var cls = new ClassElement { Name = "AllTypes" };
        var primitiveTypes = new[]
        {
            DataType.Bool, DataType.Byte, DataType.Int32, DataType.Int64,
            DataType.Single, DataType.Double, DataType.Decimal,
            DataType.Char, DataType.String, DataType.Guid,
            DataType.DateTime, DataType.DateOnly, DataType.TimeSpan,
            DataType.Object, DataType.Void
        };

        foreach (var dt in primitiveTypes)
        {
            cls.Properties.Add(new PropertyElement
            {
                Name = $"Prop{cls.Properties.Count}",
                Type = new TypeModel { BaseType = dt }
            });
        }

        var result = _generator.Generate(cls);

        result.SourceCode.Should().NotContain("/* TODO");
        result.SourceCode.Should().NotContain("/* Resolved via");
    }

    // === Array vs IsCollection precedence ===

    [Fact]
    public void Generate_ArrayWithIsCollection_ArrayWins()
    {
        var cls = new ClassElement { Name = "Order" };
        cls.Properties.Add(new PropertyElement
        {
            Name = "Items",
            Type = new TypeModel
            {
                BaseType = DataType.Array,
                IsCollection = true,
                GenericArguments = { TypeModel.Int32 }
            }
        });

        var result = _generator.Generate(cls);

        // Array cesta se vyhodnotí před IsCollection → T[] syntax
        result.SourceCode.Should().Contain("int[]");
        result.SourceCode.Should().NotContain("List<int>");
    }
}
