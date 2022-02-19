using System.Runtime.CompilerServices;

namespace Net7Sample;
internal class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        LogHelper.ConfigureLogging(builder => builder.AddConsole());
    }
}
