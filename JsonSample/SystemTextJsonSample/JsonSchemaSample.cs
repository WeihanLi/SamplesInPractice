using Json.Schema;
using WeihanLi.Common;
using WeihanLi.Common.Http;

namespace SystemTextJsonSample;

public class JsonSchemaSample
{
    public static void MainTest()
    {
        JsonSchemaBuildTest();

        JsonSchemaLoadValidateTest().Wait();
    }

    private static void JsonSchemaBuildTest()
    {
        var jsonSchema = new JsonSchemaBuilder()
            .Properties(
                ("name", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .MinLength(1)
                    .MaxLength(10)
                ),
                ("age", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Number)
                    .Minimum(1)
                )
                )
            .Required("name")
            .Build();

        var schemaString = JsonSerializer.Serialize(jsonSchema);
        WriteLine(schemaString);
        WriteLine();

        var validateResults = jsonSchema.Validate("{}");
        WriteLine(validateResults.IsValid);
    }

    private static async Task JsonSchemaLoadValidateTest()
    {
        const string testJsonSchema = @"
{
  ""$schema"": ""https://json-schema.org/draft/2020-12/schema"",
  ""type"": ""object"",
  ""properties"": {
    ""Data"": {
      ""type"": ""array"",
      ""items"":
        {
          ""type"": ""object"",
          ""properties"": {
            ""NoticeTitle"": {
              ""type"": ""string""
            },
            ""NoticeCustomPath"": {
              ""type"": ""string""
            },
            ""NoticePublishTime"": {
              ""type"": ""string""
            }
          },
          ""required"": [
            ""NoticeTitle"",
            ""NoticeCustomPath"",
            ""NoticePublishTime""
          ]
        }
    },
    ""PageNumber"": {
      ""type"": ""integer""
    },
    ""PageSize"": {
      ""type"": ""integer""
    },
    ""TotalCount"": {
      ""type"": ""integer""
    },
    ""PageCount"": {
      ""type"": ""integer""
    },
    ""Count"": {
      ""type"": ""integer""
    }
  },
  ""required"": [
    ""Data"",
    ""PageNumber"",
    ""PageSize"",
    ""TotalCount"",
    ""PageCount"",
    ""Count""
  ]
}
";
        var validJson = @"{
  ""Data"": [
    {
      ""NoticeTitle"": ""春节放假通知"",
      ""NoticeCustomPath"": ""www.123.com"",
      ""NoticePublishTime"": ""2022-02-08T08:58:41.4741144""
    }
  ],
  ""PageNumber"": 1,
  ""PageSize"": 10,
  ""TotalCount"": 5,
  ""PageCount"": 1,
  ""Count"": 5
}";
        var invalidJson = @"{
  ""Data"": [
    {
      ""NoticeExternalLink"": null
    }
  ],
  ""PageNumber"": 1,
  ""PageSize"": 10,
  ""TotalCount"": 5,
  ""PageCount"": 1,
  ""Count"": 5
}
";
        var schema = JsonSchema.FromText(testJsonSchema);

        var validationOptions = new ValidationOptions()
        {
            OutputFormat = OutputFormat.Detailed
        };

        var validateResult = schema.Validate(validJson, validationOptions);
        WriteLine(validateResult.IsValid);
        WriteLine(validateResult.Message);

        validateResult = schema.Validate(invalidJson, validationOptions);
        WriteLine(validateResult.IsValid);
        WriteLine(validateResult.Message);
        WriteLine();

        using var httpClient = new HttpClient(new NoProxyHttpClientHandler());
        var result = await httpClient.GetStringAsync("http://reservation.weihanli.xyz/api/notice");
        validateResult = schema.Validate(result, validationOptions);
        WriteLine(validateResult.IsValid);
        WriteLine(validateResult.Message);
    }
}
