using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace GetStarted;

public sealed class MainViewModel : ObservableObject
{
    public string? Input
    {
        get => $"【{field}】";
        set
        {
            field = value?.Trim();
            OnPropertyChanged();
        }
    }
    
    public RelayCommand<string> TextUpdateCommand { get; }

    public MainViewModel()
    {
        TextUpdateCommand = new RelayCommand<string>(text =>
        {
            Input = text;
        });
    }
}

public partial class MainWindow : Window
{
    private MainViewModel Model { get; } = new();
    public MainWindow()
    {
        InitializeComponent();
        Model.Input = TextBox.Text = "Hello Avalonia";
        DataContext = Model;
    }
}
