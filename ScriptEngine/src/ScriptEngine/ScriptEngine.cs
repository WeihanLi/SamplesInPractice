using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#if NETCOREAPP

using System.Runtime.Loader;

#endif

namespace ScriptEngine
{
    public class ScriptEngine
    {
        private static readonly ConcurrentDictionary<string, Type> _variableTypes = new ConcurrentDictionary<string, Type>();
        private static readonly ConcurrentDictionary<string, ScriptRunner<bool>> _conditionScripts = new ConcurrentDictionary<string, ScriptRunner<bool>>();

        private static object GetParam(string variablesInfo, string paramsInfo)
            => GetParam(variablesInfo, string.IsNullOrEmpty(paramsInfo) ? new JObject() : JObject.Parse(paramsInfo));

        private static object GetParam(string variablesInfo, JObject paramInfo)
        {
            if (string.IsNullOrEmpty(variablesInfo) || paramInfo == null)
                return null;

            var variableType = _variableTypes.GetOrAdd(variablesInfo, x =>
            {
                var variables = JsonConvert.DeserializeObject<VariableInfo[]>(variablesInfo);

                var className = $"DynamicVariable_{Guid.NewGuid():N}";

                var classCodeBuilder = new StringBuilder();
                classCodeBuilder.AppendLine("using System;");
                classCodeBuilder.AppendLine("using System.Collections.Generic;");
                classCodeBuilder.AppendLine("using System.Linq;");
                classCodeBuilder.AppendLine();
                classCodeBuilder.AppendLine("namespace ScriptEngine");
                classCodeBuilder.AppendLine("{");

                classCodeBuilder.AppendLine($"\tpublic class {className}");
                classCodeBuilder.AppendLine("\t{");

                foreach (var variable in variables)
                {
                    classCodeBuilder.AppendLine($"\t\tpublic {GetPropertyType(variable.Type)} {variable.Name} {{ get; set; }}");
                }

                classCodeBuilder.AppendLine("\t}");

                classCodeBuilder.AppendLine("}");

                var classCode = classCodeBuilder.ToString();

                var syntaxTree = CSharpSyntaxTree.ParseText(classCode, new CSharpParseOptions(LanguageVersion.Latest));

                // https://github.com/dotnet/roslyn/issues/34111
                var references =
                    new[]
                        {
                            typeof(object).Assembly,
                            typeof(Enumerable).Assembly,
                            typeof(List<>).Assembly,
#if NETCOREAPP
		                    Assembly.Load("netstandard"),
                            Assembly.Load("System.Runtime"),
#endif
                        }
                        .Select(assembly => assembly.Location)
                        .Distinct()
                        .Select(l => MetadataReference.CreateFromFile(l))
                        .Cast<MetadataReference>()
                        .ToArray();

                //var assemblyName = $"ScriptEngine.DynamicGenerated";
                var assemblyName = $"ScriptEngine.DynamicGenerated.{Guid.NewGuid():N}";

                var compilation = CSharpCompilation.Create(assemblyName)
                    .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release))
                    .AddReferences(references)
                    .AddSyntaxTrees(syntaxTree);

                var tempDir = Path.Combine(Directory.GetCurrentDirectory(), "temp");
                if (!Directory.Exists(tempDir))
                {
                    Directory.CreateDirectory(tempDir);
                }
                var dllPath = Path.Combine(tempDir, $"{assemblyName}.dll");
                var compilationResult = compilation.Emit(dllPath);
                if (compilationResult.Success)
                {
#if NETCOREAPP
                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dllPath);
#else
                    var assembly = Assembly.LoadFile(dllPath);

#endif
                    var type = assembly.GetExportedTypes().First(t => t.Name.Equals(className));
                    return type;
                }

                var error = new StringBuilder(compilationResult.Diagnostics.Length * 1024);
                foreach (var t in compilationResult.Diagnostics)
                {
                    error.AppendLine($"{t.GetMessage()}");
                }
                throw new ArgumentException($"所选文件编译有错误{Environment.NewLine}{error}");
            });

            var variableInstance = Activator.CreateInstance(variableType);

            if (null != variableInstance)
            {
                foreach (var param in paramInfo.Properties())
                {
                    var prop = variableType.GetProperty(param.Name);
                    if (null != prop && prop.CanWrite)
                    {
                        prop.SetValue(variableInstance, param.Value.ToObject(prop.PropertyType));
                    }
                }
            }
            return variableInstance;
        }

        private static string GetPropertyType(string type)
        {
            switch (type)
            {
                case "number":
                    return "int";
            }

            return type;
        }

        public static Task<bool> EvalAsync(string condition, string variables, object parameters)
            => EvalAsync(condition, variables, JObject.FromObject(parameters));

        public static async Task<bool> EvalAsync(string condition, string variables, JObject parameters)
        {
            if (string.IsNullOrEmpty(condition))
                return true;

            var param = GetParam(variables, parameters);
            var scriptOptions = ScriptOptions.Default
                .WithReferences(typeof(Enumerable).Assembly)
                .WithImports(
                    "System",
                    "System.Linq"
                );

            try
            {
                var script = _conditionScripts.GetOrAdd(condition, con => CSharpScript.Create<bool>(condition, options: scriptOptions, globalsType: param?.GetType()).CreateDelegate());
                var result = await script.Invoke(param);

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static async Task<bool> EvalAsync(string condition, string variables, string parameters)
        {
            if (string.IsNullOrEmpty(condition))
                return true;

            var param = GetParam(variables, parameters);
            var scriptOptions = ScriptOptions.Default
                .WithReferences(typeof(Enumerable).Assembly)
                .WithImports(
                  "System",
                  "System.Linq"
                );

            try
            {
                var script = _conditionScripts.GetOrAdd(condition, con => CSharpScript.Create<bool>(condition, options: scriptOptions, globalsType: param?.GetType()).CreateDelegate());
                var result = await script.Invoke(param);

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
