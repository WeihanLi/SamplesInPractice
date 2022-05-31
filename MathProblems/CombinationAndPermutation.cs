using WeihanLi.Common.Helpers.Combinatorics;
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
        foreach (var p in numbers.Partitions(2))
        {
            Console.WriteLine(p.Select(x => x.StringJoin(",")).StringJoin("\t"));
        }
        Console.WriteLine();
        Console.ReadLine();

        foreach (var split in Enumerable.Range(1, 5)
            .Select(i => new[] { 1, 2, 3, 4, 5 }.Partitions(i)))
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
