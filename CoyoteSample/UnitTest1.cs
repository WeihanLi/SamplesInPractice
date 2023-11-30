using Microsoft.Coyote;
using Microsoft.Coyote.SystematicTesting;

namespace CoyoteSample;

public class UnitTest1
{
    private readonly Helper _helper = new();

    [Fact]    
    public async Task ReadyTest()
    {
        Assert.False(_helper.Ready);
        await Task.Run(_helper.MarkAsReady);
        Assert.True(_helper.Ready);
    }

    [Test]
    [Fact]
    public void CoyoteTestTask()
    {
        var configuration = Configuration.Create().WithTestingIterations(1_000_000);
        var engine = TestingEngine.Create(configuration, ReadyTest);
        engine.Run();
    }
}

public class Helper
{
    private volatile bool _ready;

    public bool Ready => _ready;

    public void MarkAsReady() => _ready = true;
}
