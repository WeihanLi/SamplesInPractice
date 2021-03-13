using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AspNetCoreSample.Extensions
{
    public class Base64EncodedJsonInputFormatter : TextInputFormatter
    {
        public Base64EncodedJsonInputFormatter()
        {
            SupportedMediaTypes.Add("text/base64-json");
            SupportedEncodings.Add(Encoding.UTF8);
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            try
            {
                // using var reader = new StreamReader(context.HttpContext.Request.Body, encoding);
                using var reader = context.ReaderFactory(context.HttpContext.Request.Body, encoding);
                var rawContent = await reader.ReadToEndAsync();
                if (string.IsNullOrEmpty(rawContent))
                {
                    return await InputFormatterResult.NoValueAsync();
                }
                var bytes = Convert.FromBase64String(rawContent);
                var services = context.HttpContext.RequestServices;

                var modelValue = await GetModelValue(services, bytes);
                return await InputFormatterResult.SuccessAsync(modelValue);

                async ValueTask<object> GetModelValue(IServiceProvider serviceProvider, byte[] stringBytes)
                {
                    var newtonJsonOption = serviceProvider.GetService<IOptions<MvcNewtonsoftJsonOptions>>()?.Value;
                    if (newtonJsonOption is null)
                    {
                        await using var stream = new MemoryStream(stringBytes);
                        var result = await System.Text.Json.JsonSerializer.DeserializeAsync(stream, context.ModelType,
                            services.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions);
                        return result;
                    }

                    var stringContent = encoding.GetString(bytes);
                    return Newtonsoft.Json.JsonConvert.DeserializeObject(stringContent, context.ModelType, newtonJsonOption.SerializerSettings);
                }
            }
            catch (Exception e)
            {
                context.ModelState.TryAddModelError(string.Empty, e.Message);
                return await InputFormatterResult.FailureAsync();
            }
        }
    }
}
