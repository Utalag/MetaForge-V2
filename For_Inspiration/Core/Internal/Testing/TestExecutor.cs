using System.Reflection;
using MetaForge.Core.Internal.Testing.Models;

namespace MetaForge.Core.Internal.Testing;

internal static class TestExecutor
{
    public static async Task<InternalTestResult> ExecuteTestsAsync(Assembly assembly)
    {
        return await Task.Run(() =>
        {
            var boundaries = new List<DetectedBoundary>();
            var passed = 0;
            var failed = 0;

            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetMethods())
                {
                    if (!method.GetCustomAttributes(false).Any(a => a.GetType().Name == "FactAttribute"))
                        continue;

                    try
                    {
                        var instance = Activator.CreateInstance(type);
                        method.Invoke(instance, null);
                        passed++;
                    }
                    catch (Exception ex)
                    {
                        failed++;

                        if (ex.InnerException != null)
                        {
                            boundaries.Add(new DetectedBoundary(
                                Condition: method.Name.Replace("Test_", ""),
                                ExceptionType: ex.InnerException.GetType().Name,
                                Description: ex.InnerException.Message,
                                Severity: BoundaryTestSeverity.Error
                            ));
                        }
                    }
                }
            }

            return new InternalTestResult
            {
                Boundaries = boundaries,
                PassedCount = passed,
                FailedCount = failed
            };
        });
    }
}
