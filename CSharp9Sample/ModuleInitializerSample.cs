using System;
using System.Runtime.CompilerServices;

namespace CSharp9Sample
{
    internal static class ModuleInitializerSample
    {
        /// <summary>
        /// Initializer for specific module
        ///
        /// Must be static
        /// Must be parameter-less
        /// Must return void
        /// Must not be a generic method
        /// Must not be contained in a generic class
        /// Must be accessible from the containing module
        /// </summary>
        [ModuleInitializer]
        public static void Initialize()
        {
            Console.WriteLine($"{nameof(ModuleInitializerAttribute)} works");
        }
    }
}
