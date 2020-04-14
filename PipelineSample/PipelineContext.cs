namespace PipelineSample
{
    public interface IPipelineContext
    {
    }

    public class RequestContext : IPipelineContext
    {
        public string RequesterName { get; set; }

        public int Hour { get; set; }
    }
}
