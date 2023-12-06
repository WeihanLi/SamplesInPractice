namespace SignalRSample;

public class HelloModel
{
    public string Name { get; set; } = string.Empty;
    public string? Message { get; set; }
}

public interface IHelloClient
{
    Task Hello(HelloModel model);
}

public interface IHelloServer
{
    Task Hello(HelloModel model);
}
