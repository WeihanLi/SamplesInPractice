using System;
using System.Threading.Tasks;
using WeihanLi.Common.Helpers;
using WeihanLi.Extensions;
using WeihanLi.Extensions.Dump;

namespace RedisSample
{
    public class SimpleStreamUsage
    {
        private const string StreamKey = "test-simple-stream";

        public static async Task MainTest()
        {
            await RedisHelper.GetRedisDb().KeyDeleteAsync(StreamKey);
            // register background consumer
            _ = Task.Factory.StartNew(Consume).ConfigureAwait(false);
            //
            await Publish();
        }

        private static async Task Publish()
        {
            Console.WriteLine("Press Enter to publish messages, Press Q to exit");
            var input = Console.ReadLine();
            while (input is not "q" and not "Q")
            {
                var redis = RedisHelper.GetRedisDb();
                for (var i = 0; i < 10; i++)
                {
                    await redis.StreamAddAsync(StreamKey, "message", $"test_message_{i}");
                }
                input = Console.ReadLine();
            }
        }

        private static async Task Consume()
        {
            var lastMsgId = "0-0";
            while (true)
            {
                await InvokeHelper.TryInvokeAsync(async () =>
                {
                    var redis = RedisHelper.GetRedisDb();
                    var entries = await redis.StreamReadAsync(StreamKey, lastMsgId, 2);
                    if (entries.Length == 0)
                    {
                        return;
                    }
                    foreach (var entry in entries)
                    {
                        Console.WriteLine(entry.Id);
                        entry.Values.Dump();
                        // delete message if you want
                        // redis.StreamDelete(StreamKey, new[] { entry.Id });
                    }

                    lastMsgId = entries[^1].Id;
                });
                await Task.Delay(200);
            }
        }
    }
}
