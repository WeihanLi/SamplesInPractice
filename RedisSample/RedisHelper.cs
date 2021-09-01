using StackExchange.Redis;

namespace RedisSample
{
    internal static class RedisHelper
    {
        private static readonly IConnectionMultiplexer ConnectionMultiplexer = StackExchange.Redis.ConnectionMultiplexer.Connect("127.0.0.1:6379");

        public static IDatabase GetRedisDb(int dbIndex = 0)
        {
            return ConnectionMultiplexer.GetDatabase(dbIndex);
        }
    }
}
