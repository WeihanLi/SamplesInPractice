using System;
using System.Linq;
using WeihanLi.Extensions;

var list = Enumerable.Range(1, 10).ToList();

// Index/Range support
Enumerable.Range(1, 10).ElementAt(^2).Dump(); // returns 9
Enumerable.Range(1, 10).Take(^2..).Dump(); // returns [9,10]
Enumerable.Range(1, 10).Take(..2).Dump(); // returns [1,2]
Enumerable.Range(1, 10).Take(2..4).Dump(); // returns [3,4]

// DistinctBy/UnionBy/IntersectBy/ExceptBy
Enumerable.Range(1, 20).DistinctBy(x => x % 3).Dump(); // {1, 2, 3}
var first = new (string Name, int Age)[] { ("Francis", 20), ("Lindsey", 30), ("Ashley", 40) };
var second = new (string Name, int Age)[] { ("Claire", 30), ("Pat", 30), ("Drew", 33) };
first.UnionBy(second, person => person.Age).Dump(); // { ("Francis", 20), ("Lindsey", 30), ("Ashley", 40), ("Drew", 33) }

// MaxBy/MinBy
var people = new (string Name, int Age)[] { ("Francis", 20), ("Lindsey", 30), ("Ashley", 40) };
people.MaxBy(person => person.Age).Dump(); // ("Ashley", 40)
people.MinBy(x => x.Name).Dump(); // ("Ashley", 40)

// Chunk
var nums = list.Chunk(3);
nums.Dump();

// FirstOrDefault/LastOrDefault/SingleOrDefault
Enumerable.Empty<int>().FirstOrDefault(-1).Dump();
Enumerable.Empty<int>().SingleOrDefault(-1).Dump();
Enumerable.Empty<int>().LastOrDefault(-1).Dump();

// Zip
var xs = Enumerable.Range(1, 5).ToArray();
var ys = xs.Select(x => x.ToString());
var zs = xs.Select(x => x % 2 == 0);

//xs.Zip(ys,zs).Select(( x,y,z) => $"{x},{y},{z}").Dump();
foreach (var (x,y,z) in xs.Zip(ys, zs))
{
    $"{x},{y},{z}".Dump();
}
