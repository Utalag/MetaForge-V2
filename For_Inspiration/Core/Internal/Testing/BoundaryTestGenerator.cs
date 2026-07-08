using System.Text;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Primitives;
using MetaForge.Core.Elements.Types;
using MetaForge.Core.Internal.Testing.Models;

namespace MetaForge.Core.Internal.Testing;

internal static class BoundaryTestGenerator
{
    public static string Generate(Method method)
    {
        var sb = new StringBuilder();
        var className = CSharpMethodGenerator.SanitizeClassName(method.Name);

        sb.AppendLine($"namespace MetaForge.Internal.Tests {{");
        sb.AppendLine($"    public class {className}_BoundaryTests {{");

        var testCases = GenerateTestCases(method);

        foreach (var testCase in testCases)
        {
            sb.AppendLine($"        [Fact]");
            sb.AppendLine($"        public void {testCase.MethodName}() {{");
            sb.AppendLine($"            // Boundary test: {testCase.Description}");

            if (string.IsNullOrWhiteSpace(testCase.ExpectedException))
            {
                sb.AppendLine($"            var instance = new global::MetaForge.Internal.Generated.{className}_Method();");
                sb.AppendLine($"            {testCase.CallCode}");
            }
            else
            {
                sb.AppendLine($"            try {{");
                sb.AppendLine($"                var instance = new global::MetaForge.Internal.Generated.{className}_Method();");
                sb.AppendLine($"                {testCase.CallCode}");
                sb.AppendLine($"                throw new Xunit.Sdk.XunitException(\"Expected exception {testCase.ExpectedException} was not thrown.\");");
                sb.AppendLine($"            }}");
                sb.AppendLine($"            catch (Exception ex) {{");
                sb.AppendLine($"                // Expected: {testCase.ExpectedException}");
                sb.AppendLine($"                if (ex.GetType().Name != \"{testCase.ExpectedException}\") {{");
                sb.AppendLine($"                    throw new Xunit.Sdk.ThrowsException(typeof({testCase.ExpectedException}), ex);");
                sb.AppendLine($"                }}");
                sb.AppendLine($"            }}");
            }

            sb.AppendLine($"        }}");
        }

        sb.AppendLine($"    }}");
        sb.AppendLine($"}}");

        return sb.ToString();
    }

    private static List<TestCaseInfo> GenerateTestCases(Method method)
    {
        var cases = new List<TestCaseInfo>();

        foreach (var param in method.Parameters)
        {
            var boundaries = GetBoundaryValuesFromStrongType(param);

            foreach (var boundary in boundaries)
            {
                var callArgs = method.Parameters.Select(p =>
                    p.Name == param.Name ? boundary.Value : CSharpMethodGenerator.GetDefaultValue(p.Type));

                cases.Add(new TestCaseInfo
                {
                    MethodName = $"Test_{param.Name}_{boundary.Name}",
                    Description = $"{param.Name}: {boundary.Name}",
                    CallCode = $"instance.{method.Name}({string.Join(", ", callArgs)});",
                    ExpectedException = boundary.ExpectedException
                });
            }
        }

        return cases;
    }

    private static IEnumerable<BoundaryValue> GetBoundaryValuesFromStrongType(Parameter param)
    {
        if (IsNumericType(param.Type.BaseType))
        {
            yield return new BoundaryValue("Zero", "0", "DivideByZeroException");
            yield return new BoundaryValue("Negative", "-1", "ArgumentOutOfRangeException");
            yield return new BoundaryValue("Positive", "1", null);
            yield return new BoundaryValue("MaxValue", "int.MaxValue", "OverflowException");
        }
        else if (param.Type.BaseType == DataType.String)
        {
            yield return new BoundaryValue("Null", "null", "ArgumentNullException");
            yield return new BoundaryValue("Empty", "string.Empty", "ArgumentException");
            yield return new BoundaryValue("Whitespace", "\" \"", "ArgumentException");
            yield return new BoundaryValue("Valid", "\"test\"", null);
        }
    }

    private static bool IsNumericType(DataType type) => type switch
    {
        DataType.Int or DataType.Long or DataType.Short or DataType.Byte
            or DataType.Float or DataType.Double or DataType.Decimal => true,
        _ => false
    };
}
