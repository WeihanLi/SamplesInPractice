// https://github.com/dotnet/csharplang/blob/main/proposals/raw-string-literal.md
// https://github.com/dotnet/csharplang/issues/4304
namespace CSharp11Sample;

public static class RawStringLiteral
{
    public static void MainTest()
    {
        var testString = """
HTTP/1.1 200 OK
Connection: keep-alive
Date: Wed, 23 Mar 2022 14:05:03 GMT
Server: nginx/1.14.1
Transfer-Encoding: chunked
X-dotnet-HTTPie-Duration: 720.0249ms
X-dotnet-HTTPie-ResponseTimestamp: 2022/3/23 22:05:03 +08:00
""";
        Console.WriteLine(testString);

        var rawJson = """
{
  "name": "test",
  "age": 10
}
""";
        Console.WriteLine(rawJson);

        Console.WriteLine("""<div style="color:red">Amazing .NET</div>""");

        var interpolatedString = $$"""
{
  "name": "test",
  "age": {{10}}
}
""";
        Console.WriteLine(interpolatedString);

        var oh = """"
          Ok to use """ here
          """";
        Console.WriteLine(oh);

        var ohMy = """"
          Ok to use """ here
"""";
        Console.WriteLine(ohMy);
    }
}
