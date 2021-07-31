# .NET 6 中的 LINQ 更新

## Intro

在 .NET 6 中会针对 Linq 提供更好的支持，之前可能我们通过自定义扩展方法来实现的功能，现在官方直接支持了。
Linq 将更加强大，更好地帮助我们简化应用程序代码。

## Better Index & Range support

`Index` 和 `Range` 是在 C# 8.0 开始引入的一个特性，可以帮助我们更好地定位元素的位置或者在原有集合的基础上进行切片操作，.NET 6 会更好地支持 `Index` 和 `Range` 特性以及对 Linq 更好的支持，来看下面的示例：

``` c#
Enumerable.Range(1, 10).ElementAt(^2).Dump(); // returns 9
Enumerable.Range(1, 10).Take(^2..).Dump(); // returns [9,10]
Enumerable.Range(1, 10).Take(..2).Dump(); // returns [1,2]
Enumerable.Range(1, 10).Take(2..4).Dump(); // returns [3,4]
```

## XxxBy Clause

.NET 6 将引入 `XxxBy` 来支持按照集合内的元素来进行 `Max`/`Min`/`Union`/`Distinct`/`Intersect`/`Except` 等操作。

``` c#
// DistinctBy/UnionBy/IntersectBy/ExceptBy
Enumerable.Range(1, 20).DistinctBy(x => x % 3).Dump(); // [1, 2, 3]
var first = new (string Name, int Age)[] { ("Francis", 20), ("Lindsey", 30), ("Ashley", 40) };
var second = new (string Name, int Age)[] { ("Claire", 30), ("Pat", 30), ("Drew", 33) };
first.UnionBy(second, person => person.Age).Select(x=>$"{x.Name}, {x.Age}").Dump(); // { ("Francis", 20), ("Lindsey", 30), ("Ashley", 40), ("Drew", 33) }

// MaxBy/MinBy
var people = new (string Name, int Age)[] { ("Francis", 20), ("Lindsey", 30), ("Ashley", 40) };
people.MaxBy(person => person.Age).Dump(); // ("Ashley", 40)
people.MinBy(x => x.Name).Dump(); // ("Ashley", 40)
```

## Chuck

这个功能期待已久了，简单来说就是按 BatchSize 对一个集合进行分组，分组后每个小集合的元素数量最多是 BatchSize，之前我们自己写了一个扩展方法来实现，现在可以直接使用这个扩展方法了，来看下面的示例就能够明白了：

``` c#
var list = Enumerable.Range(1, 10).ToList();
var chucks = list.Chunk(3);
chucks.Dump();

// [[1,2,3],[4,5,6],[7,8,9],[10]]
```

## Default enhancement

针对于 `FirstOrDefault`/`LastOrDefault`/`SingleOrDefault` 这几个扩展方法，之前的版本中我们是不能够指定默认值的，如果遇到 Default 的情况，会使用泛型类型的默认值，在 .NET 6 之后我们就可以指定一个默认值了，示例如下：

``` c#
Enumerable.Empty<int>().FirstOrDefault(-1).Dump();
Enumerable.Empty<int>().SingleOrDefault(-1).Dump();
Enumerable.Empty<int>().LastOrDefault(-1).Dump();
```

## Zip enhancement

``` c#
var xs = Enumerable.Range(1, 5).ToArray();
var ys = xs.Select(x => x.ToString());
var zs = xs.Select(x => x % 2 == 0);

foreach (var (x,y,z) in xs.Zip(ys, zs))
{
    $"{x},{y},{z}".Dump();
}
```

输出结果如下：

``` sh
1,1,False
2,2,True
3,3,False
4,4,True
5,5,False
```

## More

除了上面的更新之外，微软还提供了一个 `TryGetNonEnumeratedCount(out int count)` 方法来尝试获取 `Count` ，这样如果 `IEnumerable<T>` 是一个 `ICollection` 对象就能比较高效地获取 `Count`，而不用调用 `Count()` 扩展方法，不需要遍历 `IEnumerable` 对象

另外针对原来的 Min/Max 扩展方法，.NET 6 会增加一个重载，可以比较方便地指定一个比较器 

``` c#
public static TSource Min<TSource>(this IEnumerable<TSource> source, IComparer<TSource> comparer);
public static TSource Max<TSource>(this IEnumerable<TSource> source, IComparer<TSource> comparer);
public static TSource Min<TSource>(this IQueryable<TSource> source, IComparer<TSource> comparer);
public static TSource Max<TSource>(this IQueryable<TSource> source, IComparer<TSource> comparer);
```

## References

- <https://github.com/WeihanLi/SamplesInPractice/blob/master/net6sample/LinqSample/Program.cs>
- <https://devblogs.microsoft.com/dotnet/announcing-net-6-preview-4/#system-linq-enhancements>

