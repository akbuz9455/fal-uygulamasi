using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using UnityEditor.Compilation;
using UnityEngine;
using Assembly = System.Reflection.Assembly;

namespace LeastSquares.Spark
{
    public static class RuntimeCompiler
    {
        public static Diagnostic[] CheckForCompilationErrors(string code)
        {
            try
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(code);
                var root = syntaxTree.GetCompilationUnitRoot();

                // Collect the necessary references
                var references = new List<MetadataReference>();
                var addedReferences = new HashSet<string>();

                void AddReference(Assembly assembly)
                {
                    if (assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location)) return;

                    if (!addedReferences.Contains(assembly.Location))
                    {
                        references.Add(MetadataReference.CreateFromFile(assembly.Location));
                        addedReferences.Add(assembly.Location);
                    }
                }

                void AddUnityReference(UnityEditor.Compilation.Assembly assembly)
                {
                    if (!addedReferences.Contains(assembly.outputPath))
                    {
                        references.Add(MetadataReference.CreateFromFile(assembly.outputPath));
                        addedReferences.Add(assembly.outputPath);
                    }
                }

                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (var i = 0; i < assemblies.Length; ++i)
                {
                    AddReference(assemblies[i]);
                }

                var unityAssemblies = CompilationPipeline.GetAssemblies();
                for (var i = 0; i < unityAssemblies.Length; ++i)
                {
                    AddUnityReference(unityAssemblies[i]);
                }

                // Create a compilation object
                var compilation = CSharpCompilation.Create(
                    $"TempAssembly",
                    new[] { syntaxTree },
                    references,
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                // Check for errors by emitting the assembly
                using var ms = new MemoryStream();
                var result = compilation.Emit(ms);

                if (result.Success) return null;
                var failures = result.Diagnostics
                    .Where(diagnostic =>
                        diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

                return failures.ToArray();

            }
            catch (Exception e)
            {
                Debug.LogError("Error while compiling using Spark: {e}");
            }
            return null;
        }
    }
}