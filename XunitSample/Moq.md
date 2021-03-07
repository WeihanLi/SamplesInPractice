# Mock 框架 Moq 的使用

## Intro

 Moq 是 .NET 中一个很流行的 Mock 框架，使用 Mock 框架我们可以只针对我们关注的代码进行测试，对于依赖项使用 Mock 对象配置预期的依赖服务的行为。

Moq 是基于 `Castle` 的动态代理来实现的，基于动态代理技术动态生成满足指定行为的类型

> 在一个项目里, 我们经常需要把某一部分程序独立出来以便我们可以对这部分进行测试. 这就要求我们不要考虑项目其余部分的复杂性, 我们只想关注需要被测试的那部分. 这里就需要用到模拟(Mock)技术.
>
> 因为, 请仔细看. 我们想要隔离测试的这部分代码对外部有一个或者多个依赖. 所以编写测试代码的时候, 我们需要提供这些依赖. 而针对隔离测试, 并不应该使用生产时用的依赖项, 所以我们使用模拟版本的依赖项, 这些模拟版依赖项只能用于测试时, 它们会使隔离更加容易.
>
> ![img](https://images2018.cnblogs.com/blog/986268/201807/986268-20180711160423444-549906338.png)
>
> **绿色**的是需要被测试的类,**黄色**是**Mock**的依赖项
>
> ——引用自杨旭大佬的博文

## Prepare

首先我们需要先准备一下用于测试的类和接口，下面的示例都是基于下面定义的类和方法来做的

``` c#
public interface IUserIdProvider
{
    string GetUserId();
}
public class TestModel
{
    public int Id { get; set; }
}
public interface IRepository
{
    int Version { get; set; }

    int GetCount();

    Task<int> GetCountAsync();

    TestModel GetById(int id);

    List<TestModel> GetList();

    TResult GetResult<TResult>(string sql);

    int GetNum<T>();

    bool Delete(int id);
}

public class TestService
{
    private readonly IRepository _repository;

    public TestService(IRepository repository)
    {
        _repository = repository;
    }

    public int Version
    {
        get => _repository.Version;
        set => _repository.Version = value;
    }

    public List<TestModel> GetList() => _repository.GetList();

    public TResult GetResult<TResult>(string sql) => _repository.GetResult<TResult>(sql);

    public int GetResult(string sql) => _repository.GetResult<int>(sql);

    public int GetNum<T>() => _repository.GetNum<T>();

    public int GetCount() => _repository.GetCount();

    public Task<int> GetCountAsync() => _repository.GetCountAsync();

    public TestModel GetById(int id) => _repository.GetById(id);

    public bool Delete(TestModel model) => _repository.Delete(model.Id);
}
```

我们要测试的类型就是类似 `TestService` 这样的，而 `IRepositoy<TestModel>` 和 `IUserIdProvider` 是属于外部依赖

## Mock Method

### Get Started

通常我们使用 Moq 最常用的可能就是 Mock 一个方法了，最简单的一个示例如下：

``` c#
[Fact]
public void BasicTest()
{
    var userIdProviderMock = new Mock<IUserIdProvider>();
    userIdProviderMock.Setup(x => x.GetUserId()).Returns("mock");
    Assert.Equal("mock", userIdProviderMock.Object.GetUserId());
}
```

### Match Arguments

通常我们的方法很多是带有参数的，在使用 Moq 的时候我们可以通过设置参数匹配为不同的参数返回不同的结果，来看下面的这个例子：

``` c#
[Fact]
public void MethodParameterMatch()
{
    var repositoryMock = new Mock<IRepository>();
    repositoryMock.Setup(x => x.Delete(It.IsAny<int>()))
        .Returns(true);
    repositoryMock.Setup(x => x.GetById(It.Is<int>(_ => _ > 0)))
        .Returns((int id) => new TestModel()
        {
            Id = id
        });

    var service = new TestService(repositoryMock.Object);
    var deleted = service.Delete(new TestModel());
    Assert.True(deleted);

    var result = service.GetById(1);
    Assert.NotNull(result);
    Assert.Equal(1, result.Id);

    result = service.GetById(-1);
    Assert.Null(result);

    repositoryMock.Setup(x => x.GetById(It.Is<int>(_ => _ <= 0)))
        .Returns(() => new TestModel()
        {
            Id = -1
        });
    result = service.GetById(0);
    Assert.NotNull(result);
    Assert.Equal(-1, result.Id);
}
```

通过 `It.IsAny<T>` 来表示匹配这个类型的所有值，通过 `It.Is<T>(Expression<Func<bool>>)` 来设置一个表达式来断言这个类型的值

通过上面的例子，我们可以看的出来，设置返回值的时候，可以直接设置一个固定的返回值，也可以设置一个委托来返回一个值，也可以根据方法的参数来动态配置返回结果

### Async Method

现在很多地方都是在用异步方法，Moq 设置异步方法有三种方式，一起来看一下示例：

``` c#
[Fact]
public async Task AsyncMethod()
{
    var repositoryMock = new Mock<IRepository>();

    // Task.FromResult
    repositoryMock.Setup(x => x.GetCountAsync())
        .Returns(Task.FromResult(10));
    // ReturnAsync
    repositoryMock.Setup(x => x.GetCountAsync())
        .ReturnsAsync(10);
    // Mock Result, start from 4.16
    repositoryMock.Setup(x => x.GetCountAsync().Result)
        .Returns(10);

    var service = new TestService(repositoryMock.Object);
    var result = await service.GetCountAsync();
    Assert.True(result > 0);
}
```

还有一个方式也可以，但是不推荐，编译器也会给出一个警告，就是下面这样

``` c#
repositoryMock.Setup(x => x.GetCountAsync()).Returns(async () => 10);
```

### Generic Type

有些方法会是泛型方法，对于泛型方法，我们来看下面的示例：

``` c#
[Fact]
public void GenericType()
{
    var repositoryMock = new Mock<IRepository>();
    var service = new TestService(repositoryMock.Object);

    repositoryMock.Setup(x => x.GetResult<int>(It.IsAny<string>()))
        .Returns(1);
    Assert.Equal(1, service.GetResult(""));

    repositoryMock.Setup(x => x.GetResult<string>(It.IsAny<string>()))
        .Returns("test");
    Assert.Equal("test", service.GetResult<string>(""));
}

[Fact]
public void GenericTypeMatch()
{
    var repositoryMock = new Mock<IRepository>();
    var service = new TestService(repositoryMock.Object);

    repositoryMock.Setup(m => m.GetNum<It.IsAnyType>())
        .Returns(-1);
    repositoryMock.Setup(m => m.GetNum<It.IsSubtype<TestModel>>())
        .Returns(0);
    repositoryMock.Setup(m => m.GetNum<string>())
        .Returns(1);
    repositoryMock.Setup(m => m.GetNum<int>())
        .Returns(2);

    Assert.Equal(0, service.GetNum<TestModel>());
    Assert.Equal(1, service.GetNum<string>());
    Assert.Equal(2, service.GetNum<int>());
    Assert.Equal(-1, service.GetNum<byte>());
}
```

如果要 `Mock` 指定类型的数据，可以直接指定泛型类型，如上面的第一个测试用例，如果要不同类型设置不同的结果一种是直接设置类型，如果要指定某个类型或者某个类型的子类，可以用 `It.IsSubtype<T>`，如果要指定值类型可以用 `It.IsValueType`，如果要匹配所有类型则可以用 `It.IsAnyType`

### Callback

我们在设置 Mock 行为的时候可以设置 callback 来模拟方法执行时的逻辑，来看一下下面的示例：

``` c#
[Fact]
public void Callback()
{
    var deletedIds = new List<int>();
    var repositoryMock = new Mock<IRepository>();
    var service = new TestService(repositoryMock.Object);
    repositoryMock.Setup(x => x.Delete(It.IsAny<int>()))
        .Callback((int id) =>
        {
            deletedIds.Add(id);
        })
        .Returns(true);

    for (var i = 0; i < 10; i++)
    {
        service.Delete(new TestModel() { Id = i });
    }
    Assert.Equal(10, deletedIds.Count);
    for (var i = 0; i < 10; i++)
    {
        Assert.Equal(i, deletedIds[i]);
    }
}
```

### Verification

有时候我们会验证某个方法是否执行，并不需要关注是否方法的返回值，这时我们可以使用 `Verification` 验证某个方法是否被调用，示例如下：

``` c#
[Fact]
public void Verification()
{
    var repositoryMock = new Mock<IRepository>();
    var service = new TestService(repositoryMock.Object);

    service.Delete(new TestModel()
    {
        Id = 1
    });

    repositoryMock.Verify(x => x.Delete(1));
    repositoryMock.Verify(x => x.Version, Times.Never());
    Assert.Throws<MockException>(() => repositoryMock.Verify(x => x.Delete(2)));
}
```

如果方法没有被调用，就会引发一个 `MockException` 异常：

![verification failed](https://img2020.cnblogs.com/blog/489462/202103/489462-20210308000409309-407440519.png)

`Verification` 也可以指定方法触发的次数，比如：`repositoryMock.Verify(x => x.Version, Times.Never);`，默认是 `Times.AtLeastOnce`，可以指定具体次数 `Times.Exactly(1)` 或者指定一个范围 `Times.Between(1,2, Range.Inclusive)`，Moq 也提供了一些比较方便的方法，比如`Times.Never()`/`Times.Once()`/`Times.AtLeaseOnce()`/`Times.AtMostOnce()`/`Times.AtLease(2)`/`Times.AtMost(2)`

## Mock Property

Moq 也可以 mock 属性，property 的本质是方法加一个字段，所以也可以用 Mock 方法的方式来 Mock 属性，只是使用 Mock 方法的方式进行 Mock 属性的话，后续修改属性值就不会引起属性值的变化了，如果修改属性，则要使用 `SetupProperty` 的方式来 Mock 属性，具体可以参考下面的这个示例：

``` c#
[Fact]
public void Property()
{
    var repositoryMock = new Mock<IRepository>();
    var service = new TestService(repositoryMock.Object);
    repositoryMock.Setup(x => x.Version).Returns(1);
    Assert.Equal(1, service.Version);

    service.Version = 2;
    Assert.Equal(1, service.Version);
}

[Fact]
public void PropertyTracking()
{
    var repositoryMock = new Mock<IRepository>();
    var service = new TestService(repositoryMock.Object);
    repositoryMock.SetupProperty(x => x.Version, 1);
    Assert.Equal(1, service.Version);

    service.Version = 2;
    Assert.Equal(2, service.Version);
}
```

## Sequence

我们可以通过 `Sequence` 来指定一个方法执行多次返回不同结果的效果，看一下示例就明白了：

``` c#
[Fact]
public void Sequence()
{
    var repositoryMock = new Mock<IRepository>();
    var service = new TestService(repositoryMock.Object);

    repositoryMock.SetupSequence(x => x.GetCount())
        .Returns(1)
        .Returns(2)
        .Returns(3)
        .Throws(new InvalidOperationException());

    Assert.Equal(1, service.GetCount());
    Assert.Equal(2, service.GetCount());
    Assert.Equal(3, service.GetCount());
    Assert.Throws<InvalidOperationException>(() => service.GetCount());
}
```

第一次调用返回值是1，第二次是2，第三次是3，第四次是抛了一个 `InvalidOperationException`

## LINQ to Mocks

我们可以通过 `Mock.Of` 来实现类似 LINQ 的方式，创建一个 mock 对象实例，指定类型的实例，如果对象比较深，要 mock 的对象比较多使用这种方式可能会一定程度上简化自己的代码，来看使用示例：

``` c#
[Fact]
public void MockLinq()
{
    var services = Mock.Of<IServiceProvider>(sp =>
        sp.GetService(typeof(IRepository)) == Mock.Of<IRepository>(r => r.Version == 1) &&
        sp.GetService(typeof(IUserIdProvider)) == Mock.Of<IUserIdProvider>(a => a.GetUserId() == "test"));

    Assert.Equal(1, services.ResolveService<IRepository>().Version);
    Assert.Equal("test", services.ResolveService<IUserIdProvider>().GetUserId());
}
```

## Mock Behavior

默认的 Mock Behavior 是 `Loose`，默认没有设置预期行为的时候不会抛异常，会返回方法返回值类型的默认值或者空数组或者空枚举，

在声明 `Mock` 对象的时候可以指定 Behavior 为 `Strict`，这样就是一个**"真正"**的 mock 对象，没有设置预期行为的时候就会抛出异常，示例如下：

``` c#
[Fact]
public void MockBehaviorTest()
{
    // Make mock behave like a "true Mock",
    // raising exceptions for anything that doesn't have a corresponding expectation: in Moq slang a "Strict" mock;
    // default behavior is "Loose" mock,
    // which never throws and returns default values or empty arrays, enumerable, etc

    var repositoryMock = new Mock<IRepository>();
    var service = new TestService(repositoryMock.Object);
    Assert.Equal(0, service.GetCount());
    Assert.Null(service.GetList());
    
    var arrayResult = repositoryMock.Object.GetArray();
    Assert.NotNull(arrayResult);
    Assert.Empty(arrayResult);

    repositoryMock = new Mock<IRepository>(MockBehavior.Strict);
    Assert.Throws<MockException>(() => new TestService(repositoryMock.Object).GetCount());
}
```

使用 `Strict` 模式不设置预期行为的时候就会报异常，异常信息类似下面这样：

![strict exception](https://img2020.cnblogs.com/blog/489462/202103/489462-20210308000408830-2135599526.png)

## More

Moq 还有一些别的用法，还支持事件的操作，还有 Protected 成员的 Mock，还有一些高级的用法，自定义 Default 行为等，感觉我们平时可能并不太常用，所以上面并没有加以介绍，有需要用的可以参考 Moq 的文档

上述测试代码可以在 Github 获取 <https://github.com/WeihanLi/SamplesInPractice/blob/master/XunitSample/MoqTest.cs>

## References

- <https://github.com/moq/moq4/wiki/Quickstart>
- <https://github.com/moq/moq4>
- <https://github.com/WeihanLi/SamplesInPractice/blob/master/XunitSample/MoqTest.cs>
- <https://www.cnblogs.com/tylerzhou/p/11410337.html>
- <https://www.cnblogs.com/cgzl/p/9304567.html>
- <https://www.cnblogs.com/haogj/archive/2011/07/22/2113496.html>
