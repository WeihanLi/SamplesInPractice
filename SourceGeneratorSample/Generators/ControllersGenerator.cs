using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Generators
{
    [Generator]
    public class ControllersGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var appDbContextType = context.Compilation.GetTypeByMetadataName("AspNetCoreWebDemo.Models.AppDbContext");
            // 从编译信息中获取 DbSet<> 类型
            var dbContextType = context.Compilation.GetTypeByMetadataName(typeof(DbSet<>).FullName);
            // 获取 DbContext 中的 DbSet<> 属性
            var propertySymbols = appDbContextType.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(x => x.MethodKind == MethodKind.PropertyGet
                        && x.ReturnType is INamedTypeSymbol
                        {
                            IsGenericType: true,
                            IsUnboundGenericType: false,
                        } typeSymbol
                        && ReferenceEquals(typeSymbol.ConstructedFrom.ContainingAssembly, dbContextType.ContainingAssembly)
                        )
                .ToArray()
                ;
            // 获取属性的返回值
            var propertyReturnType = propertySymbols
                .Select(r =>
                    Tuple.Create((INamedTypeSymbol)r.ReturnType, r.Name.Replace("get_", "", StringComparison.OrdinalIgnoreCase))
                    )
                .ToArray();
            // 获取属性泛型类型参数，并获取泛型类型参数的名称
            (string typeName, string propertyName)[] models = propertyReturnType
                .Select(t => (t.Item1.TypeArguments.First().Name, t.Item2))
                .ToArray();

            //Debugger.Launch();

            foreach (var additionalFile in context.AdditionalFiles)
            {
                var templateName = Path.GetFileNameWithoutExtension(additionalFile.Path).Replace("Template", "");
                var template = additionalFile.GetText(context.CancellationToken);

                if (template is not null)
                {
                    foreach (var (typeName, propertyName) in models)
                    {
                        var code = template.ToString()
                            .Replace("{PropertyName}", propertyName, StringComparison.OrdinalIgnoreCase)
                            .Replace("{TypeName}", typeName, StringComparison.OrdinalIgnoreCase)
                            ;

                        context.AddSource($"{propertyName}{templateName}", code);
                    }
                }
            }
        }
    }
}
