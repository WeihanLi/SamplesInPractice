using System.Reflection;
using System.Reflection.Emit;

namespace Net9Samples;

public static class AssemblyBuilderSampler
{
    public static void MainTest()
    {
        var testAssemblyName = $"test-{Guid.NewGuid():N}";
        var assemblyBuilder = AssemblyBuilder.DefinePersistedAssembly(new AssemblyName(testAssemblyName), typeof(object).Assembly);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("Main");
        var typeBuilder = moduleBuilder.DefineType("TestType", TypeAttributes.Public | TypeAttributes.Sealed);
        var type = typeBuilder.CreateType();
        type.GetMethod("Test")?.Invoke(null, []);
        assemblyBuilder.Save($"{testAssemblyName}.dll");
    }
}
