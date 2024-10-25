using System.Threading.RateLimiting;

{
    var rateLimiter = new ConcurrencyLimiter(new()
    {
        PermitLimit = 3,
        QueueLimit = 3
    });
    Console.WriteLine(DateTimeOffset.Now);
    Parallel.For(1, 40, _ =>
    {
        using var lease = rateLimiter.AttemptAcquire();
        if (lease.IsAcquired)
        {
            Console.WriteLine("Successfully acquired lease");
        }
        else
        {
            Console.WriteLine("Failed to acquire lease");
            Thread.Sleep(TimeSpan.FromSeconds(5));
        }
    });
    Console.WriteLine(DateTimeOffset.Now);
}

{
    var rateLimiter = new FixedWindowRateLimiter(new ()
    {
        PermitLimit = 3,
        QueueLimit = 3,
        Window = TimeSpan.FromSeconds(5)
    });
    Console.WriteLine(DateTimeOffset.Now);
    Parallel.For(1, 40, _ =>
    {
        using var lease = rateLimiter.AttemptAcquire();
        if (lease.IsAcquired)
        {
            Console.WriteLine("Successfully acquired lease");
        }
        else
        {
            Console.WriteLine("Failed to acquire lease");
            Thread.Sleep(TimeSpan.FromSeconds(5));
        }
    });
    Console.WriteLine(DateTimeOffset.Now);
}

{
    var rateLimiter = new SlidingWindowRateLimiter(new ()
    {
        PermitLimit = 3,
        QueueLimit = 3,
        Window = TimeSpan.FromSeconds(5),
        SegmentsPerWindow = 5
    });
    Console.WriteLine(DateTimeOffset.Now);
    Parallel.For(1, 40, _ =>
    {
        using var lease = rateLimiter.AttemptAcquire();
        if (lease.IsAcquired)
        {
            Console.WriteLine("Successfully acquired lease");
        }
        else
        {
            Console.WriteLine("Failed to acquire lease");
            Thread.Sleep(TimeSpan.FromSeconds(5));
        }
    });
    Console.WriteLine(DateTimeOffset.Now);
}

{
    var rateLimiter = new TokenBucketRateLimiter(new()
    {
        AutoReplenishment = true, 
        QueueLimit = 1,
        ReplenishmentPeriod = TimeSpan.FromSeconds(10),
        TokenLimit = 5,
        TokensPerPeriod = 3,
    });
    Console.WriteLine(DateTimeOffset.Now);
    Parallel.For(1, 40, _ =>
    {
        using var lease = rateLimiter.AttemptAcquire();
        if (lease.IsAcquired)
        {
            Console.WriteLine("Successfully acquired lease");
        }
        else
        {
            Console.WriteLine("Failed to acquire lease");
            Thread.Sleep(rateLimiter.ReplenishmentPeriod);
        }
    });
    Console.WriteLine(DateTimeOffset.Now);
}
Console.WriteLine("Completed!");
