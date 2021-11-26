namespace CSharp10Sample;

// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-10.0/async-method-builders
public class AsyncMethodBuilderAttributeSample
{
    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))] // new usage of AsyncMethodBuilderAttribute type
    public static ValueTask<int> ExampleAsync()
    {
        return ValueTask.FromResult(0);
    }
}
