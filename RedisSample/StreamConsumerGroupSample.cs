using System;
using System.Threading;
using System.Threading.Tasks;
using WeihanLi.Common.Helpers;
using WeihanLi.Extensions;

namespace RedisSample
{
    internal class StreamConsumerGroupSample
    {
        private const string StreamKey = "test-stream-group";
        private static int _consumerCount;

        public static async Task MainTest()
        {
            await RedisHelper.GetRedisDb().KeyDeleteAsync(StreamKey);
            // register background consumer
            _ = await Task.Factory.StartNew(Consume).ConfigureAwait(false);
            _ = await Task.Factory.StartNew(Consume).ConfigureAwait(false);
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
            Interlocked.Increment(ref _consumerCount);
            var groupName = $"group-{_consumerCount}";
            var consumerName = $"consumer-{_consumerCount}";
            var redis = RedisHelper.GetRedisDb();
            redis.StreamCreateConsumerGroup(StreamKey, groupName);
            while (true)
            {
                await InvokeHelper.TryInvokeAsync(async () =>
                {
                    var messages = await redis.StreamReadGroupAsync(StreamKey, groupName, consumerName, count: SecurityHelper.Random.Next(1, 4));
                    if (messages.Length == 0)
                    {
                        return;
                    }
                    foreach (var message in messages)
                    {
                        Console.WriteLine($"{groupName}-{message.Id}-{message.Values.ToJson()}");
                        await redis.StreamAcknowledgeAsync(StreamKey, groupName, message.Id);
                    }
                });
                await Task.Delay(200);
            }
        }
    }
}
