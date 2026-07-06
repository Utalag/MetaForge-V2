using FluentAssertions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Types;
using MetaForge.Generators;

namespace MetaForge.Core.Integration.Tests.Scenarios;

/// <summary>
/// Snapshot testy pro TypeModel varianty v Property — T1-T18,T22,T23 z matice.
/// </summary>
public class TypeModelVariantsSnapshots
{
    private readonly CodeGenerator _generator = new();

    [Fact] public void T01_String() => GenerateAndVerify("Name", TypeModel.String, "string");
    [Fact] public void T02_Int32() => GenerateAndVerify("Id", TypeModel.Int32, "int");
    [Fact] public void T03_NullableInt() => GenerateAndVerify("Age", TypeModel.Int32.MakeNullable(), "int?");
    [Fact] public void T04_NullableString() => GenerateAndVerify("Nickname", TypeModel.String.MakeNullable(), "string?");
    [Fact] public void T05_IntCollection() => GenerateAndVerify("Ids", TypeModel.Int32.MakeCollection(), "List<object>");
    [Fact] public void T06_StringCollection() => GenerateAndVerify("Tags", TypeModel.String.MakeCollection(), "List<object>");
    [Fact] public void T08_Bool() => GenerateAndVerify("IsActive", TypeModel.Bool, "bool");
    [Fact] public void T09_Decimal() => GenerateAndVerify("Price", TypeModel.Decimal, "decimal");
    [Fact] public void T10_Guid() => GenerateAndVerify("Key", TypeModel.Guid, "Guid");
    [Fact] public void T11_DateTime() => GenerateAndVerify("Created", TypeModel.DateTime, "DateTime");
    [Fact] public void T12_Double() => GenerateAndVerify("Rate", TypeModel.Of(DataType.Double), "double");
    [Fact] public void T13_Int64() => GenerateAndVerify("BigId", TypeModel.Of(DataType.Int64), "long");
    [Fact] public void T14_Byte() => GenerateAndVerify("Flag", TypeModel.Of(DataType.Byte), "byte");
    [Fact] public void T15_DateOnly() => GenerateAndVerify("Birth", TypeModel.Of(DataType.DateOnly), "DateOnly");
    [Fact] public void T16_TimeSpan() => GenerateAndVerify("Duration", TypeModel.Of(DataType.TimeSpan), "TimeSpan");
    [Fact] public void T17_Object() => GenerateAndVerify("Data", TypeModel.Object, "object");
    [Fact] public void T18_Dynamic() => GenerateAndVerify("Payload", TypeModel.Of(DataType.Dynamic), "dynamic");
    [Fact] public void T22_CustomType() => GenerateAndVerify("CustomerId", TypeModel.Int32.WithCustomName("CustomerId"), "CustomerId");

    private void GenerateAndVerify(string propName, TypeModel propType, string expectedCSharpType)
    {
        var cls = ClassElement.Basic("Entity");
        cls.Properties.Add(PropertyElement.GetSet(propName, propType));
        var result = _generator.Generate(cls);

        SnapshotComparer.Verify("TypeModel", propName, result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain(expectedCSharpType);
    }
}
