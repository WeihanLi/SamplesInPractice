using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using System;
using Xunit;

namespace XunitSample;

//     <PackageReference Include="FluentAssertions" Version="6.12.0" />
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
    
    [Fact]
    public void DateTest()
    {
        var date = DateOnly.FromDateTime(DateTime.Today);
        date.Should().HaveYear(DateTime.Now.Year);
        date.Should().HaveMonth(DateTime.Now.Month);
        date.Should().HaveDay(DateTime.Now.Day);
    }

    [Fact]
    public void ExtensionTest()
    {
        var model = new TestModel() { Name = "Hello" };
        model.Should().WithName("Hello");
        model.Should().WithNamePrefix("He").And.WithName("Hello");
    }
}

file sealed class TestModel
{
    public required string Name { get; init; }
}

file sealed class TestModelAssertions : ReferenceTypeAssertions<TestModel, TestModelAssertions>
{
    public TestModelAssertions(TestModel subject) : base(subject)
    {
    }

    protected override string Identifier { get; } = "Name";

    public AndConstraint<TestModelAssertions> WithName(string name, string because = "", params object[] becauseArgs)
    {
        Subject.Name.Should().Be(name, because, becauseArgs);
        return new AndConstraint<TestModelAssertions>(this);
    }
    
    public AndConstraint<TestModelAssertions> WithNamePrefix(string prefix, string because = "", params object[] becauseArgs)
    {
        Execute.Assertion.BecauseOf(because, becauseArgs)
            .ForCondition(Subject.Name.StartsWith(prefix))
            .FailWith("Name should had prefix {0}", prefix);
        return new AndConstraint<TestModelAssertions>(this);
    }
}

file static class TestModelAssertionsExtensions
{
    public static TestModelAssertions Should(this TestModel testModel) => new TestModelAssertions(testModel);
}
