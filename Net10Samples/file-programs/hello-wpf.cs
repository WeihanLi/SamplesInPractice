#:property TargetFramework=net10.0-windows
#:property UseWPF=true
#:property PublishAot=false

System.Windows.MessageBox.Show("Hello, WPF!");

// using System.Windows;
// using System.Windows.Controls;

// public static class Program
// {
//     [STAThread]
//     public static void Main()
//     {
//         var app = new Application();
//         var window = new Window
//         {
//             Title = "Hello WPF",
//             Width = 400,
//             Height = 250,
//             WindowStartupLocation = WindowStartupLocation.CenterScreen,
//             Content = new TextBlock
//             {
//                 Text = "Hello, World!",
//                 FontSize = 30,
//                 HorizontalAlignment = HorizontalAlignment.Center,
//                 VerticalAlignment = VerticalAlignment.Center
//             }
//         };
//         app.Run(window);
//     }
// }
