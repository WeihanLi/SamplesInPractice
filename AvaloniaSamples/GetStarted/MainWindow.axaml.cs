using Avalonia.Controls;

namespace GetStarted;

public partial class MainWindow : Window
{
    private string? Input
    {
        get;
        set => field = value?.Trim();
    }

    public MainWindow()
    {
        InitializeComponent();
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
    }
}
