using FluentAssertions;
using FluentAssertions.Execution;
using System;
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
        "Hello World".Should().StartWith("Hello ").And.EndWith("World");
        "Hello World".Should().Be("Hello World");
    }
    
    [Fact]
    public void ExceptionTest()
    {
        var action = Helper.ArgumentExceptionTest;
        action.Should().Throw<ArgumentException>();
        action.Should().ThrowExactly<ArgumentException>();
        
        var action2 = Helper.ArgumentNullExceptionTest;
        action2.Should().Throw<ArgumentNullException>();
        action2.Should().Throw<ArgumentException>();
        action2.Should().ThrowExactly<ArgumentNullException>();
    }
    
    [Fact]
    public void AssertScopeTest()
    {
        using (new AssertionScope())
        {
            var action = Helper.ArgumentExceptionTest;
            action.Should().Throw<ArgumentException>();
            action.Should().ThrowExactly<ArgumentException>();
        }

        using (new AssertionScope())
        {
            var action2 = Helper.ArgumentNullExceptionTest;
            action2.Should().Throw<ArgumentNullException>();
            action2.Should().Throw<ArgumentException>();
            action2.Should().ThrowExactly<ArgumentNullException>();
        }
    }
}
