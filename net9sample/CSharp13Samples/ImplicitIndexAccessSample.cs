namespace CSharp13Samples;
// The implicit "from the end" index operator, ^, is now allowed in an object initializer expression. 
// https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#implicit-index-access
internal class ImplicitIndexAccessSample
{
    public static void MainTest()
    {
        //int[] numbers =
        //{
        //    [^1] = 4,
        //    [^2] = 3,
        //    [^3] = 2,
        //    [^4] = 1,
        //};
        //Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(numbers));
    }
}
