#: property LangVersion preview
#: package Avalonia@11.3.7
#: package Avalonia.Desktop@11.3.7
#: package Avalonia.Themes.Fluent@11.3.7

using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Themes.Fluent;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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
   
sealed class MainViewModel : INotifyPropertyChanged
{
    public string? Input
    {
        get => $"[{field}]";
        set
        {
            field = value?.Trim();
            OnPropertyChanged();
        }
    }

    public void TextChanged(object? sender, TextChangedEventArgs? textChangedEventArgs)
    {
        if (textChangedEventArgs?.Source is TextBox inputTextBox)
            Input = inputTextBox.Text;
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

class MainWindow : Window
{
    private readonly MainViewModel ViewModel = new();

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
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            FontSize = 24,
            Height = 80
        };
        Content = new StackPanel
        {
            Children =
            {
                textBox,
                textBlock
            }
        };
        DataContext = ViewModel;
        textBox.TextChanged += ViewModel.TextChanged;
        textBlock.Bind(TextBlock.TextProperty, new Binding(nameof(MainViewModel.Input)));
    }
}
