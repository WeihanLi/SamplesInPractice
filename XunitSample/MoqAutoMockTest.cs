using Moq;
using Moq.AutoMock;
using Xunit;

namespace XunitSample;

public class MoqAutoMockerTest
{
    [Fact]
    public void TestMethod1WithoutAutoMocker()
    {
        var mockA = new Mock<IA>();
        var service = new TestService(mockA.Object, new Mock<IB>().Object, new Mock<IC>().Object,
            new Mock<ID>().Object);
        mockA.Setup(x => x.GetResult())
            .Returns(1);
        var result = service.TestMethod1();
        Assert.Equal(1, result);
    }

    [Fact]
    public void TestMethod1WithAutoMocker()
    {
        var autoMocker = new AutoMocker();
        autoMocker.GetMock<IA>().Setup(x => x.GetResult())
            .Returns(1);
        var service = autoMocker.CreateInstance<TestService>();
        var result = service.TestMethod1();
        Assert.Equal(1, result);
    }

    [Fact]
    public void TestMethod2()
    {
        var autoMocker = new AutoMocker();
        autoMocker.GetMock<IB>().Setup(x => x.GetResult())
            .Returns(1);
        var cMock = new Mock<IC>();
        cMock.Setup(x => x.GetResult())
            .Returns(1);
        autoMocker.Use(cMock);
        // autoMocker.Use(cMock.Object);
        var service = autoMocker.CreateInstance<TestService>();
        var result = service.TestMethod2();
        Assert.Equal(2, result);
        autoMocker.VerifyAll();
    }

    [Fact]
    public void TestMethod3()
    {
        var autoMocker = new AutoMocker();
        autoMocker.Use<ID>(x => x.GetResult() == 1);
        var service = autoMocker.CreateInstance<TestService>();
        var result = service.TestMethod3();
        Assert.Equal(101, result);
        autoMocker.GetMock<ID>()
            .Verify(x => x.GetResult());
    }
}

public interface I0
{
    int GetResult();
}

public interface IA : I0
{
}

public interface IB : I0
{
}

public interface IC : I0
{
}

public interface ID : I0
{
}

file sealed class TestService
{
    private readonly IA _a;
    private readonly IB _b;
    private readonly IC _c;
    private readonly ID _d;

    public TestService(IA a, IB b, IC c, ID d)
    {
        _a = a;
        _b = b;
        _c = c;
        _d = d;
    }

    public int TestMethod1()
    {
        return _a.GetResult();
    }

    public int TestMethod2()
    {
        return _b.GetResult() + _c.GetResult();
    }

    public int TestMethod3()
    {
        return _d.GetResult() + 100;
    }
}
