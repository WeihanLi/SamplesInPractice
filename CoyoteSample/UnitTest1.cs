using Microsoft.Coyote.SystematicTesting;

namespace CoyoteSample;

public class UnitTest1
{
    [Test]
    [Fact]
    public async Task NoLockReadyTest()
    {
        var _helper = new Helper();
        Assert.False(_helper.Ready);
        await Task.Run(_helper.MarkAsReady);
        Assert.True(_helper.Ready);
    }

    [Test]
    [Fact]
    public async Task LockReadyTest()
    {
        var _helper = new HelperV2();
        Assert.False(_helper.Ready);
        await Task.Run(_helper.MarkAsReady);
        Assert.True(_helper.Ready);
    }
}

public class Helper
{
    private volatile bool _ready;

    public bool Ready => _ready;

    public void MarkAsReady()
    {
        _ready = true;
    }
}


public class HelperV2
{
    private readonly object _readyLock = new();
    private volatile bool _ready;

    public bool Ready
    {
        get 
        {
            if (_ready) return true;

            lock(_readyLock)
            {
                if (_ready) return true;
                
                return _ready;
            }
        }
    }

    public void MarkAsReady()
    {
        lock (_readyLock)
        {
            _ready = true;
        }
    }
}
