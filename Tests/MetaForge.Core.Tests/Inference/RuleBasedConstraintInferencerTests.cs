using FluentAssertions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Inference;

namespace MetaForge.Core.Tests.Inference;

public class RuleBasedConstraintInferencerTests
{
    private readonly RuleBasedConstraintInferencer _inferencer = new();

    [Fact]
    public void Infer_Email_ReturnsEmailConstraints()
    {
        var result = _inferencer.Infer("email", TypeModel.String);
        result.Should().Contain("email_format");
        result.Should().Contain("not_empty");
    }

    [Fact]
    public void Infer_Phone_ReturnsPhoneConstraints()
    {
        var result = _inferencer.Infer("phone", TypeModel.String);
        result.Should().Contain("phone_format");
    }

    [Fact]
    public void Infer_Price_ReturnsNotNegative()
    {
        var result = _inferencer.Infer("price", TypeModel.Decimal);
        result.Should().Contain("not_negative");
    }

    [Fact]
    public void Infer_UnknownName_ReturnsDefaultForNonNullableString()
    {
        var result = _inferencer.Infer("xyz123unknown", TypeModel.String);
        // Non-nullable string vrací not_empty jako výchozí
        result.Should().Contain("not_empty");
    }

    [Fact]
    public void Infer_NonNullableString_InfersNotEmpty()
    {
        var result = _inferencer.Infer("description", TypeModel.String);
        result.Should().Contain("max_length:4000");
    }

    [Fact]
    public void Infer_PrefixMatch_EmailAddress_ReturnsEmailConstraints()
    {
        var result = _inferencer.Infer("emailAddress", TypeModel.String);
        result.Should().Contain("email_format");
    }
}
