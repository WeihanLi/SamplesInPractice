namespace CSharp13Samples;

// https://github.com/dotnet/csharplang/blob/main/proposals/csharp-13.0/lock-object.md
// https://github.com/dotnet/runtime/issues/34812
// https://github.com/dotnet/csharplang/issues/7104
public class LockObjectSample
{
    public static void MainTest()
    {
        var i = 0;
        var locker = new Lock();
        Parallel.For(1, 100, _ =>
        {
            lock (locker)
            {
                i++;
            }
        });
        Console.WriteLine(i);
    }
}

internal class CustomLock
{
    public Scope EnterLockScope()
    {
        return new Scope();
    }

    public ref struct Scope
    {
        public void Dispose()
        {
        }
    }
}
