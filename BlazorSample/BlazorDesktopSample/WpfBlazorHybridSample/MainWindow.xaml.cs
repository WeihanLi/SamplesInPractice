using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Extensions.DependencyInjection;
using WeihanLi.Common;

namespace WpfBlazorHybridSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly AppState _appState = new();
        private readonly IServiceProvider _serviceProvider;
        public MainWindow()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddBlazorWebView();
            serviceCollection.AddSingleton(_appState);
            _serviceProvider = serviceCollection.BuildServiceProvider();
            DependencyResolver.SetDependencyResolver(_serviceProvider);
            Resources.Add("services", _serviceProvider);

            InitializeComponent();
        }

        private void btnCount_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Current counter is {_appState.Counter}");
        }
    }
}
