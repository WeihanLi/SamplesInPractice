using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CSharp9Sample
{
    internal static class AwaitableExtensions
    {
        private static ValueTaskAwaiter<int> GetAwaiter(this int t)
        {
            return ValueTask.FromResult(t).GetAwaiter();
        }

        public static async void MainTest()
        {
            await 1;
        }
    }
}
