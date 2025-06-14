#: property LangVersion preview
#: package Avalonia@11.3.1
#: package Avalonia.Desktop@11.3.1
#: package Avalonia.Themes.Fluent@11.3.1

using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;

AppBuilder.Configure<App>()
    .UsePlatformDetect()
    .StartWithClassicDesktopLifetime(args)
    ;

class App : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
    }
    
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
     private string? Input
    {
        get;
        set => field = value?.Trim();
    }

    public MainWindow()
    {
        Title = "Hello Avalonia";
        Width = 400;
        Height = 300;
        var textBox = new TextBox
        {
            Name = "InputTextBox",
            Text = "Hello, Avalonia!",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Width = 400,
            Height = 60
        };
        var textBlock = new TextBlock
        {
            Name = "OutputTextBlock",
            Text = "Hello, Avalonia!",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            FontSize = 24,
            Height = 80
        };

        Input = textBox.Text;
        textBlock.Text = $"[{Input}]";
        textBox.TextChanged += (sender, args) =>
        {
            if (sender is TextBox inputTextBox)
            {
                Input = inputTextBox.Text;
                textBlock.Text = $"[{Input}]";
            }
        };
        var panel = new StackPanel
        {
            Margin = new Thickness(20)
        };
        panel.Children.Add(textBox);
        panel.Children.Add(textBlock);

        Content = panel;
    }
}
