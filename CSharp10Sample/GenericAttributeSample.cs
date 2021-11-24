namespace CSharp10Sample;

public class GenericAttributeSample
{
    // Error   CS0655	'DataValiadator' is not a valid named attribute argument because it is not a valid attribute parameter type
    // [ExcelConfiguration<TestModel>(DataValiadator = m => m.Id > 0)]
    public class TestModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ExcelConfigurationAttribute<T> : Attribute
    {
        public string DefaultFileName { get; set; } = "unnamed-file.xlsx";
        public Func<T, bool> DataValiadator { get; set; } = _ => true;
    }
}
