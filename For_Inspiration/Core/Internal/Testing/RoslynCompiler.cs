using System.Diagnostics;
using System.Reflection;
using MetaForge.Core.Internal.Testing.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using MetaForge.Core.Elements.Members;

namespace MetaForge.Core.Internal.Testing;

internal static class RoslynCompiler
{
    public static async Task<CompilationResult> CompileAsync(string methodCode, string testCode, Method method)
    {
        return await Task.Run(() =>
        {
            var syntaxTreeMethod = CSharpSyntaxTree.ParseText(methodCode);
            var syntaxTreeTests = CSharpSyntaxTree.ParseText(testCode);

            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Debug).Assembly.Location),
            };

            var systemRef = Assembly.Load("System.Runtime").Location;
            references.Add(MetadataReference.CreateFromFile(systemRef));

            var compilation = CSharpCompilation.Create(
                $"MetaForge.Test.{method.Name}",
                new[] { syntaxTreeMethod, syntaxTreeTests },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using var ms = new MemoryStream();
            var emitResult = compilation.Emit(ms);

            if (emitResult.Success)
            {
                ms.Seek(0, SeekOrigin.Begin);
                var assembly = Assembly.Load(ms.ToArray());
                return new CompilationResult { Success = true, Assembly = assembly };
            }

            return new CompilationResult { Success = false, Diagnostics = emitResult.Diagnostics };
        });
    }
}
