using System.Windows;
using Microsoft.Win32;

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
            OpenFileDialog ofd = new OpenFileDialog { Filter = "XML Filse|*.xml", Multiselect = true };
            if (ofd.ShowDialog() == true)
            {
                foreach (string fname in ofd.FileNames)
                {
                    var ap = new AddProtocolWindow(fname);
                    ap.Left = Left;
                    ap.Top = Top;
                    if (ap.Protocol != null)
                        ap.ShowDialog();
                }
            }
        }
    }
}