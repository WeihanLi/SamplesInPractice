# ScriptEngine

## Intro

C# 脚本引擎，用来动态解析 C# 脚本

## Condition Eval

条件解析示例：

``` csharp
[Fact]
public async Task EvalTest()
{
    var condition = "x+y > 10";
    var variables = JsonConvert.SerializeObject(new[]
    {
        new
        {
            Name = "x",
            Type = "int"
        },
        new
        {
            Name = "y",
            Type = "int"
        },
    });

    var params1 = new Dictionary<string, object>()
    {
        { "x", 2 },
        { "y", 3 }
    };
    Assert.False(await ScriptEngine.EvalAsync(condition, variables, params1));

    var params1_1 = JsonConvert.SerializeObject(params1);
    Assert.False(await ScriptEngine.EvalAsync(condition, variables, params1_1));

    var params2 = new
    {
        x = 6,
        y = 5
    };
    Assert.True(await ScriptEngine.EvalAsync(condition, variables, params2));
}

[Fact]
public async Task EvalStringTest()
{
    var condition = "x > y.Length";
    var variables = JsonConvert.SerializeObject(new[]
    {
        new
        {
            Name = "x",
            Type = "int"
        },
        new
        {
            Name = "y",
            Type = "string"
        },
    });

    var params1 = new
    {
        x = 1,
        y = "3"
    };
    Assert.False(await ScriptEngine.EvalAsync(condition, variables, params1));

    var params2 = new
    {
        x = 6,
        y = "5211"
    };
    Assert.True(await ScriptEngine.EvalAsync(condition, variables, params2));
}

[Fact]
public async Task EvalLinqTest()
{
    var condition = "list.Any(x=>x>10)";
    var variables = JsonConvert.SerializeObject(new[]
    {
        new
        {
            Name = "list",
            Type = "List<int>"
        }
    });

    var params1 = new
    {
        list = new List<int>()
        {
            1,2,3,4,5
        }
    };
    Assert.False(await ScriptEngine.EvalAsync(condition, variables, params1));

    var params2 = new
    {
        list = new List<int>()
        {
            1,2,3,4,5,10,12
        }
    };
    Assert.True(await ScriptEngine.EvalAsync(condition, variables, params2));
}
```

## 实现原理

实现的方式是基于 Roslyn 实现的，核心实现是基于 Roslyn 的 Script 实现的，但是 Roslyn Script 的执行有一些限制，不支持匿名类对象的解析，因此还基于 Roslyn 运行时根据变量信息来动态生成一个类型用于执行脚本解析

``` csharp
var result = await CSharpScript.EvaluateAsync<bool>("1 > 2");
```

运行时动态生成代码在之前的 DbTool 项目中介绍过，介绍文章 [基于 Roslyn 实现动态编译](https://www.cnblogs.com/weihanli/p/dynamic-compile-via-roslyn.html)

详细实现细节可以参考代码

## Memo

### 程序集加载在 framework 和 core 环境下的差异

实现的时候我们的项目有 dotnetcore 的，还有 netframework 的，这两者加载 dll 的时候略有不同，实现的时候用了一个条件编译，在 dotnet core 环境下和 dotnet framework 分开处理，在 dotnetcore 中使用 `AssemblyLoadContext` 来加载程序集

``` csharp
#if NETCOREAPP
    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dllPath);
#else
    var assembly = Assembly.LoadFile(dllPath);
#endif
```

### 程序集要保存到文件

原本打算动态生成的程序集保存的一个 Stream 不保存文件，但是实际测试下来必须要保存到文件才可以，所以在项目根目录下创建了一个临时目录 `temp` 用来保存动态生成的程序集

### Roslyn 动态生成的程序集管理

目前还是比较简单的放在一个 `temp` 目录下了，总觉得每一个类型生成一个程序集有些浪费，但是好像也没办法修改已有程序集，还没找到比较好的解决方案，如果有好的处理方式，欢迎一起交流

## More

Natasha 是一个基于 Roslyn 来实现动态编译，能够让你更方便进行动态操作，有动态编译相关需求的可以关注一下这个项目，后面也想用 Natasha 来优化前面提到的问题

> 基于roslyn的动态编译库，为您提供高效率、高性能、可追踪的动态构建方案，兼容stanadard2.0, 只需原生C#语法不用Emit。 让您的动态方法更加容易编写、跟踪、维护

## Reference

- <https://github.com/dotnet/roslyn/blob/main/docs/wiki/Scripting-API-Samples.md>
- <https://github.com/dotnetcore/Natasha>
