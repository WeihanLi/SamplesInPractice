using System.Runtime.Versioning;

[assembly:RequiresPreviewFeatures]
namespace Net9Samples;

public static class LockSample
{
    public static void MainTest()
    {
        Console.WriteLine(Enumerable.Range(1, 1000).Sum());
        Console.WriteLine();
        
        {
            var sum = 0;

            Parallel.For(1, 1001, i =>
            {
                sum += i;
            });

            Console.WriteLine(sum);
        }
        
        {
            var sum = 0;

            Parallel.For(1, 1001, i =>
            {
                lock(nameof(MainTest))
                {
                    sum += i;
                }
            });

            Console.WriteLine(sum);
        }
        
        {
            // This API requires opting into preview features
            var locker = new Lock();
            var sum = 0;

            Parallel.For(1, 1001, i =>
            {
                using (locker.EnterScope())
                {
                    sum += i;
                }
            });

            Console.WriteLine(sum);
        }

    }
}
