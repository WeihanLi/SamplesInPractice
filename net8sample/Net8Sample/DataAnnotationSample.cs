using System.ComponentModel.DataAnnotations;
using WeihanLi.Extensions;

namespace Net8Sample;

public static class DataAnnotationSample
{
    public static void MainTest()
    {
        var request = new Request()
        {
            CustomerId = Guid.Empty,
            RequestType = "Admin",
            UserName = "Admin",
            LuckyRate = 1,
            Image = "123",
            Items = Array.Empty<string>()
        };
        
        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();
        if (Validator.TryValidateObject(request, validationContext, validationResults, true))
        {
            Console.WriteLine("Object valid");
        }
        else
        {
            Console.WriteLine(validationResults.ToJson());
        }
    }
}

file sealed class Request
{
    [Required(DisallowAllDefaultValues = true)]
    public Guid CustomerId { get; set; }

    [AllowedValues("Dev", "Test", "Production")]
    public string RequestType { get; set; }

    [DeniedValues("Admin", "Administrator")]
    public string UserName { get; set; }

    [Range(0d, 1d, MaximumIsExclusive = true, MinimumIsExclusive = false)]
    public double LuckyRate { get; set; }
    
    [Required]
    [Base64String]
    public string Image { get; set; }

    [Length(1, 10)]
    public string[] Items { get; set; }
}
