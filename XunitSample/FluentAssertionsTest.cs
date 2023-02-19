using FluentAssertions;
using Xunit;

namespace XunitSample;

public class FluentAssertionsTest
{
    [Fact]
    public void AddTest()
    {
        Helper.Add(2, 2).Should().Be(4);
        Helper.Add(2, 2).Should().NotBe(3, "Just a sample");
        
        Helper.Add(2, 2).Should().BePositive();
    }
    
    [Fact]
    public void StringTest()
    {
        "Hello World".Should().NotBeEmpty();
        "Hello World".Should().NotBeNullOrEmpty();
        "Hello World".Should().StartWith("Hello ");
        "Hello World".Should().Be("Hello World");
    }
}
