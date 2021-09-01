using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RedisSample
{
    public class StreamTrimSample
    {
        public static async Task MainTest()
        {
            await MaxLengthTrim();
            await MinMsgIdTrim();
        }


        private static async Task MaxLengthTrim()
        {
            var streamKey = $"stream-{nameof(MaxLengthTrim)}";
            await AddStreamMessage(streamKey, 10);
            var redis = RedisHelper.GetRedisDb();
            Console.WriteLine(await redis.StreamLengthAsync(streamKey));

            // trim directly
            await redis.StreamTrimAsync(streamKey, 5);
            Console.WriteLine(await redis.StreamLengthAsync(streamKey));

            // add with trim
            await redis.StreamAddAsync(streamKey, StreamMessageField, "Test", maxLength: 3);
            Console.WriteLine(await redis.StreamLengthAsync(streamKey));

            await redis.KeyDeleteAsync(streamKey);
        }


        private const string StreamAddCommandName = "XADD";
        private const string StreamTrimCommandName = "XTRIM";

        private const string StreamAddAutoMsgId = "*";

        private const string StreamTrimByMinIdName = "MINID";

        private const string StreamTrimOperator = "=";

        private const string StreamMessageField = "message";

        private static async Task MinMsgIdTrim()
        {
            var streamKey = $"stream-{nameof(MaxLengthTrim)}";
            await AddStreamMessage(streamKey, 10, () => Thread.Sleep(1000));
            
            var redis = RedisHelper.GetRedisDb();
            var minId = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromSeconds(5)).ToUnixTimeMilliseconds();
            Console.WriteLine(await redis.StreamLengthAsync(streamKey));

            // https://redis.io/commands/xtrim
            // trim directly
            await redis.ExecuteAsync(
                StreamTrimCommandName, 
                streamKey,
                StreamTrimByMinIdName,
                StreamTrimOperator, // optional
                minId
                );
            Console.WriteLine(await redis.StreamLengthAsync(streamKey));
            minId = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromSeconds(2)).ToUnixTimeMilliseconds();

            // https://redis.io/commands/xadd
            // add with trim
            var result = redis.Execute(
                StreamAddCommandName, 
                streamKey, 
                StreamTrimByMinIdName, 
                StreamTrimOperator, // optional
                minId,
                StreamAddAutoMsgId, 
                StreamMessageField, 
                "Test"
                );
            Console.WriteLine(await redis.StreamLengthAsync(streamKey));

            await redis.KeyDeleteAsync(streamKey);
        }

        private static async Task AddStreamMessage(string key, int msgCount, Action? action=null)
        {
            var redis = RedisHelper.GetRedisDb();
            for (var i = 0; i < msgCount; i++)
            {
                await redis.StreamAddAsync(key, "messages", $"val-{i}");
                action?.Invoke();
            }
        }
    }
}
