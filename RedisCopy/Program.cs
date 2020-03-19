using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using WeihanLi.Extensions;

namespace RedisCopy
{
    public class Program
    {
        private static int batchSize;

        public static async Task Main(string[] args)
        {
            //
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            batchSize = configuration.GetAppSetting<int>("BatchSize");
            if (batchSize <= 0)
            {
                batchSize = 50;
            }

            var srcRedis = ConnectionMultiplexer.Connect(configuration.GetConnectionString("Source"));
            var destRedis = ConnectionMultiplexer.Connect(configuration.GetConnectionString("Dest"));

            var databases = configuration.GetAppSetting("SyncDatabases")
                .SplitArray<int>();
            if (databases.Length > 0)
            {
                var srcServer = srcRedis.GetServer(srcRedis.GetEndPoints()[0]);

                foreach (var database in databases)
                {
                    await SyncDatabase(srcServer, database, srcRedis, destRedis);
                }
            }

            Console.WriteLine("Completed!");
        }

        private static async Task SyncDatabase(IServer redisServer, int database, IConnectionMultiplexer srcRedis, IConnectionMultiplexer destRedis)
        {
            Console.WriteLine($"-- sync db:{database} begin --");

            var pageSize = batchSize;
            var pageIndex = 0;
            while (pageSize >= batchSize)
            {
                pageSize = 0;
                foreach (var redisKey in redisServer.Keys(database, pageSize: batchSize, pageOffset: pageIndex))
                {
                    Console.WriteLine($"-- sync db:{database},{redisKey} begin --");
                    try
                    {
                        await SyncRedisKeyAsync(srcRedis.GetDatabase(database), redisKey, destRedis.GetDatabase(database));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"sync exception:{e}");
                    }
                    finally
                    {
                        Console.WriteLine($"-- sync db:{database},{redisKey} end --");
                    }

                    pageSize++;
                }
                pageIndex++;
            }

            Console.WriteLine($"-- sync db:{database} end --");
        }

        private static async Task SyncRedisKeyAsync(IDatabase srcDatabase, RedisKey redisKey, IDatabase destDatabase)
        {
            if (destDatabase.KeyExists(redisKey))
            {
                destDatabase.KeyDelete(redisKey);
            }
            var type = await srcDatabase.KeyTypeAsync(redisKey);
            switch (type)
            {
                case RedisType.String:
                    var strVal = await srcDatabase.StringGetAsync(redisKey);
                    await destDatabase.StringSetAsync(redisKey, strVal);
                    break;

                case RedisType.List:
                    var list = await srcDatabase.ListRangeAsync(redisKey);
                    await destDatabase.ListRightPushAsync(redisKey, list);
                    break;

                case RedisType.Set:
                    var set = await srcDatabase.SetMembersAsync(redisKey);
                    await destDatabase.SetAddAsync(redisKey, set);
                    break;

                case RedisType.SortedSet:
                    var zset = await srcDatabase.SortedSetRangeByRankWithScoresAsync(redisKey);
                    await destDatabase.SortedSetAddAsync(redisKey, zset);
                    break;

                case RedisType.Hash:
                    var hash = await srcDatabase.HashGetAllAsync(redisKey);
                    await destDatabase.HashSetAsync(redisKey, hash);
                    break;

                case RedisType.Stream:
                case RedisType.Unknown:
                case RedisType.None:
                    throw new NotSupportedException($"not supported redis data type:{type}");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
