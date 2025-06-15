using Avalonia.Controls;
using Avalonia.Data;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GetStarted;

public sealed class MainViewModel : INotifyPropertyChanged
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
        {
            Input = inputTextBox.Text;
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public partial class MainWindow : Window
{
    private MainViewModel Model { get; } = new();
    public MainWindow()
    {
        InitializeComponent();
        Model.Input = textBox.Text;
        textBox.TextChanged += Model.TextChanged;
        DataContext = Model;
        textBlock.Bind(TextBox.TextProperty, new Binding(nameof(Model.Input)));
    }
}
