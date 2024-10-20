namespace CleanCSharpSamples;

public static class RawStringLiteralSample
{
    public static void Run()
    {
        {
            var html= @"<div style=""font-size:1.2em""><p>Header Text</p></div>";
            Console.WriteLine(html);
        
            html = """<div style="font-size:1.2em"><p>Header Text</p></div>""";
            Console.WriteLine(html);

            var text = "Header Text";
            html = $"""<div style="font-size:1.2em"><p>{text}</p></div>""";
            Console.WriteLine(html);
            
            var anotherText = """"Header """ Text """";
            Console.WriteLine(anotherText);
            
            var sql = """
                      SELECT [Id], [Name], [Description] 
                      FROM dbo.tbl_Users WITH(NOLOCK)
                      WHERE [IsDeleted] = 0
                      """;
            Console.WriteLine(sql);
            
            sql = """
                  SELECT [Id], [Name], [Description] 
                  FROM dbo.tbl_Users WITH(NOLOCK)
                  WHERE [IsDeleted] = 0
""";
            Console.WriteLine(sql);
        }

        {
            var json = """{"name":"Mike","age":10}""";
            Console.WriteLine(json);
            json = """
                   {
                     "name": "Mike",
                     "age": 10
                   }
                   """;
            Console.WriteLine(json);
            
            var name = "Mike";
            json = $$"""
                   {
                     "name": "{{name}}",
                     "age": 10
                   }
                   """;
            Console.WriteLine(json);
            json = $$$"""
                     {
                       "name": "{{{name}}}",
                       "title": "{{I}}",
                       "age": 10
                     }
                     """;
            Console.WriteLine(json);
        }
    }
}
