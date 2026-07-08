using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using MetaForge.Core.Elements.Members;

namespace MetaForge.Core.Internal.Testing;

public sealed class RoslynTestRunner
{
    public async Task<BoundaryTestResult> RunAsync(Method method)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var methodCode = CSharpMethodGenerator.Generate(method);

            var testCode = BoundaryTestGenerator.Generate(method);

            var compilationResult = await RoslynCompiler.CompileAsync(methodCode, testCode, method);

            if (!compilationResult.Success)
            {
                return new BoundaryTestResult
                {
                    MethodId = method.GetHashCode().ToString(),
                    Version = 1,
                    Boundaries = Array.Empty<DetectedBoundary>(),
                    PassedCount = 0,
                    FailedCount = 0,
                    Duration = stopwatch.Elapsed,
                    ResultHash = ComputeHash("")
                };
            }

            var testResult = await TestExecutor.ExecuteTestsAsync(compilationResult.Assembly!);

            stopwatch.Stop();

            return new BoundaryTestResult
            {
                MethodId = method.GetHashCode().ToString(),
                Version = 1,
                Boundaries = testResult.Boundaries,
                PassedCount = testResult.PassedCount,
                FailedCount = testResult.FailedCount,
                Duration = stopwatch.Elapsed,
                ResultHash = ComputeHash(testResult.Boundaries)
            };
        }
        catch
        {
            stopwatch.Stop();
            return new BoundaryTestResult
            {
                MethodId = method.GetHashCode().ToString(),
                Version = 1,
                Boundaries = Array.Empty<DetectedBoundary>(),
                PassedCount = 0,
                FailedCount = 0,
                Duration = stopwatch.Elapsed,
                ResultHash = ComputeHash("")
            };
        }
    }

    private static string ComputeHash(string content)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(bytes);
    }

    private static string ComputeHash(IEnumerable<DetectedBoundary> boundaries)
    {
        var content = string.Join("|", boundaries.Select(b => $"{b.Condition}:{b.ExceptionType}"));
        return ComputeHash(content);
    }
}
