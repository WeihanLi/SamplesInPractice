﻿using Microsoft.SemanticKernel;

namespace GetStarted;

public static class PluginSample
{
    public static async Task MainTest(string[] args)
    {
        var kernel = new KernelBuilder()
            .Build();
    }
}
