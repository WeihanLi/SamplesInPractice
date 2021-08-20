using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

Console.WriteLine(Random.Shared.Next(1, 100));

//Parallel.For(0, Environment.ProcessorCount, _ =>
//{
//    var thread = new Thread(() => 
//    {
//        var arr = new int[10];
//        for (var i = 0; i < arr.Length; i++)
//        {
//            arr[i] = Random.Shared.Next(1, 100);
//        }
//        Console.WriteLine(arr.Average());
//    });
//    thread.Start();
//});

var random = new Random();
await Task.WhenAll(
    Enumerable.Range(1, 10).Select(_ =>
        Task.Run(() => Console.WriteLine(random.Next(1, 10))))
    ).WaitAsync(TimeSpan.FromSeconds(3));

Console.WriteLine();

await Task.WhenAll(
    Enumerable.Range(1,10).Select(_ => 
       Task.Run(()=>Console.WriteLine(Random.Shared.Next(1,10))))
    ).WaitAsync(TimeSpan.FromSeconds(3));

// Console.WriteLine("Hello world");
Console.ReadLine();
