using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DiagnosticSample.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class TestController: ControllerBase
    {   
        [HttpGet]
        public IActionResult ThreadPoolStarvation(int seconds = 10)
        {
            Task.Delay(TimeSpan.FromSeconds(seconds)).Wait();
            return Ok();
        }
        
        [HttpGet]
        public async Task<IActionResult> NoThreadPoolStarvation(int seconds = 10)
        {
            await Task.Delay(TimeSpan.FromSeconds(seconds));
            return Ok();
        }
        
        [HttpGet]
        public async Task<IActionResult> HighCpu(int seconds = 60)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(seconds));
            await Task.Run(() =>
            {
                Parallel.For(1, Environment.ProcessorCount, _ =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    while (!cts.IsCancellationRequested)
                    {
                        //
                    }
                });
            }, cts.Token);
            return Ok();
        }

        private static readonly ConcurrentBag<byte[]> Bag = new();
        [HttpGet]
        public IActionResult MemoryLeak()
        {
            Bag.Add(new byte[85000]);
            return Ok(Bag.Count);
        }

        private static readonly object Lock = new();
        [HttpGet]
        public IActionResult DeadLock(int seconds = 60)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(seconds));
            cts.Token.ThrowIfCancellationRequested();
            lock (cts)
            {
                TestMethod1();
            }
            return Ok();
            
            static void TestMethod1()
            {
                lock (Lock)
                {
                    Console.WriteLine("Entered");
                }
            }
        }

        [HttpGet]
        public IActionResult StackOverflow()
        {
            Test();
            return Ok();

            // ReSharper disable once FunctionRecursiveOnAllPaths
            static void Test()
            {
                Test();
            }
        }
    }
}
