namespace CSharp11Sample;

public static class ListPatternSample
{
    public static void MainTest()
    {
        var array =
            //new List<int>()
            new[]
            {
                1,2,3,4,5
            }
            ;

        if (array is [1, ..])
        {
            Console.WriteLine("The first one is 1");
        }
        if (array is [.., 5])
        {
            Console.WriteLine("The last one is 5");
        }

        if (array is [_, _, 3, ..])
        {
            Console.WriteLine("The third one is 3");
        }
        if (array is [.., 4, _])
        {
            Console.WriteLine("The second to last one is 4");
        }

        if (array is [1, 2, 3, ..])
        {
            Console.WriteLine("The sequence starts with 1,2,3");
        }
        if (array is [.., 3, 4, 5])
        {
            Console.WriteLine("The sequence ends with 3,4,5");
        }
        if (array is [.., 3, 4, _])
        {
            Console.WriteLine("The sequence ends with 3,4,_");
        }

        var objects = new object[]
        {
            1, "2", 3.0, "4", Guid.NewGuid()
        };

        if (objects is [1, "2", ..])
        {
            Console.WriteLine($"{objects[0]},{objects[1]}");
        }

        if (objects is [.., string str, Guid guid])
        {
            Console.WriteLine($"{str},{guid:N}");
        }
        var result = objects switch
        {
            [1, ..] => 1,
            [_, "2", ..] => 2,
            [_, _, double val, ..] => (int)val,
            [.., ""] => 4,
            [.., string strVal, Guid _] => Convert.ToInt32(strVal),
            _ => -1
        };
        Console.WriteLine(result);
    }
}
