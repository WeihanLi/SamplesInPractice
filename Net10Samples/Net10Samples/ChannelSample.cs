using System.Threading.Channels;
using WeihanLi.Common.Helpers;

namespace Net10Samples;

public static class ChannelSample
{
    // https://github.com/dotnet/runtime/issues/94046
    // https://github.com/dotnet/runtime/pull/116097
    public static async Task UnbufferedChannelSample()
    {
        {
            var channel = Channel.CreateBounded<WorkItem>(0);
            var producer = Task.Run(async () =>
            {
                for (var i = 0; i < 5; i++)
                {
                    Console.WriteLine($"[Producer]准备生产项目 {i} {DateTimeOffset.Now}");
                    await channel.Writer.WriteAsync(new WorkItem(i));
                    Console.WriteLine($"[Producer]项目 {i} 已交付 {DateTimeOffset.Now}");
                }

                channel.Writer.Complete();
            });

            var consumer = Task.Run(async () =>
            {
                await foreach (var item in channel.Reader.ReadAllAsync())
                {
                    Console.WriteLine($"[Consumer]接收到项目 {item.Id} {DateTimeOffset.Now}");
                    await Task.Delay(1000); // 模拟处理
                    Console.WriteLine($"[Consumer]项目 {item.Id} 处理完成 {DateTimeOffset.Now}");
                }
            });

            await Task.WhenAll(producer, consumer);
            ConsoleHelper.ReadLineWithPrompt();
        }

        {
            var channel = Channel.CreateBounded<int>(0);
            var write1 = channel.Writer.WriteAsync(1); // 阻塞
            var write2 = channel.Writer.WriteAsync(2); // 阻塞

            Console.WriteLine(channel.Reader.Count); // 输出:  0

            ConsoleHelper.ReadLineWithPrompt();
        }

        {
            var channel = Channel.CreateBounded<string>(0);

            // 写入操作会阻塞，直到有读取者准备好接收
            var writeTask = channel.Writer.WriteAsync("test");
            // 读取操作会阻塞，直到有写入者准备好发送
            var value = await channel.Reader.ReadAsync();
            Console.WriteLine(value);
            await writeTask; // 写入完成

            _ = Task.Run(async () =>
            {
                await foreach (var message in channel.Reader.ReadAllAsync())
                {
                    Console.WriteLine($"Received message: {message} at [{DateTimeOffset.Now}]");
                }
            });

            
            for (var i = 0; i < 3; i++)
            {
                await channel.Writer.WriteAsync($"Hello World! {DateTimeOffset.Now}");
                await Task.Delay(1000);
            }
            
            ConsoleHelper.ReadLineWithPrompt();
        }

        {
            var options = new BoundedChannelOptions(0)
            {
                FullMode = BoundedChannelFullMode.DropWrite
            };
            var channel = Channel.CreateBounded<string>(options, item => Console.WriteLine($"item dropped: {item}"));
            var consumer = Task.Run(async () =>
            {
                await foreach (var message in channel.Reader.ReadAllAsync())
                {
                    Console.WriteLine($"Received message: {message} at [{DateTimeOffset.Now}]");
                    await Task.Delay(100);
                }
            });
            var producer = Task.Run(async () =>
            {
                for (var i = 0; i < 5; i++)
                {
                    await channel.Writer.WriteAsync($"Hello World! {DateTimeOffset.Now}");
                    await Task.Delay(1000);
                }

                channel.Writer.Complete();
            });
            await Task.WhenAll(producer, consumer);

            ConsoleHelper.ReadLineWithPrompt();
        }
    }
}

file sealed record WorkItem(int Id);
