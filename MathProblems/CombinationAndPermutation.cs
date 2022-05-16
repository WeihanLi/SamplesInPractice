using Combinatorics.Collections;
using WeihanLi.Extensions;

namespace MathProblems;

internal static class CombinationAndPermutation
{
    public static void MainTest()
    {
        Console.WriteLine("Hello, World!");

        // 9*9 乘法表
        //for (int i = 1; i <= 9; i++)
        //{
        //    for (int k = 1; k <= i; k++)
        //    {
        //        Console.Write("{0} * {1} = {2}", i, k, i * k);
        //        Console.Write("\t");
        //    }
        //    Console.Write(Environment.NewLine);
        //}

        // Combinations(numbers);
        // Permutations(numbers);

        var numbers = new[] { 1, 2, 3 };
        foreach (var p in Partitions(numbers, 2))
        {
            Console.WriteLine(p.Select(x => x.StringJoin(",")).StringJoin("\t"));
        }
        Console.WriteLine();
        Console.ReadLine();

        foreach (var split in Enumerable.Range(1, 5)
            .Select(i => Partitions(new[] { 1, 2, 3, 4, 5 }, i)))
        {
            var cnt = 0;
            foreach (var item in split)
            {
                Console.WriteLine(item.Select(x => x.StringJoin(",")).StringJoin(", "));
                cnt++;
            }
            Console.WriteLine(cnt);
            Console.WriteLine();
        }

        Console.WriteLine();
        Console.ReadLine();
    }

    // https://github.com/more-itertools/more-itertools/blob/master/more_itertools/more.py#L3149
    //def set_partitions_helper(L, k):
    //n = len(L)
    //if k == 1:
    //    yield [L]
    //elif n == k:
    //    yield [[s] for s in L]
    //else:
    //    e, *M = L
    //    for p in set_partitions_helper(M, k - 1):
    //        yield [[e], *p]
    //    for p in set_partitions_helper(M, k):
    //        for i in range(len(p)):
    //            yield p[:i] + [[e] + p[i]] + p[i + 1 :]
    private static IEnumerable<int[][]> Partitions(this int[] numbers, int batch)
    {
        if (batch <= 0 || numbers.Length < batch)
        {
            throw new ArgumentException("Invalid batch size", nameof(batch));
        }
        if (batch == 1)
        {
            yield return new[] { numbers };
        }
        else if (batch == numbers.Length)
        {
            yield return numbers.Select(x => new[] { x }).ToArray();
        }
        else
        {
            var e = numbers[0];
            var m = numbers[1..];
            foreach (var p in Partitions(m, batch - 1))
            {
                yield return new[]
                {
                    new []{e}
                 }.Concat(p).ToArray();
            }
            foreach (var p in Partitions(m, batch))
            {
                for (var i = 0; i < p.Length; i++)
                {
                    yield return p[..i]
                    .Concat(new[] { new[] { e }.Concat(p[i]).ToArray() })
                    .Concat(p[(i + 1)..])
                    .ToArray();
                }
            }
        }
    }

    private static void Combinations(ICollection<int> numbers)
    {
        //var list = new List<bool>();
        //list.AddRange(numbers.Select((_, i) => i < numbers.Count - 2));
        //Console.WriteLine(list.StringJoin(","));

        var combinations = new Combinations<int>(numbers, 2, GenerateOption.WithoutRepetition);
        Console.WriteLine(combinations.Count);
        foreach (var item in combinations)
        {
            Console.WriteLine(item.StringJoin(", "));
        }
    }

    private static void Permutations(ICollection<int> numbers)
    {
        var permutations = new Permutations<int>(numbers, GenerateOption.WithoutRepetition);
        Console.WriteLine(permutations.Count);
        foreach (var item in permutations)
        {
            Console.WriteLine(item.StringJoin(", "));
        }
        Console.WriteLine();
    }
}
