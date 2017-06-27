using System;
using System.Windows;
using knesset_app.DBEntities;

namespace knesset_app
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddProtocol(object sender, RoutedEventArgs e)
        {
            var ap = new AddProtocolWindow();
            if (ap.Protocol != null)
                ap.ShowDialog();
        }
    }
}
