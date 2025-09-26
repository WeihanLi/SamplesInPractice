#!/usr/bin/env dotnet
#: package Avalonia@11.3.1
#: package Avalonia.Desktop@11.3.1

using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System.Runtime.InteropServices;

AppBuilder.Configure<App>()
    .UsePlatformDetect()
    .StartWithClassicDesktopLifetime(args)
    ;

class App : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}

class MainWindow : Window
{
    public MainWindow()
    {
        Title = "Hello Avalonia";
        Width = 500;
        Height = 300;
        Content = new TextBlock
        {
            Text = $"""
                    Hello Avalonia on {RuntimeInformation.OSDescription}({RuntimeInformation.RuntimeIdentifier})
                          {RuntimeInformation.FrameworkDescription}
                    """,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            FontSize = 24
        };
    }
}
