using knesset_app.DBEntities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shapes;

namespace knesset_app
{
    /// <summary>
    /// Interaction logic for AddProtocolWindow.xaml
    /// </summary>
    public partial class AddProtocolWindow : Window
    {

        internal Protocol protocol { get; set; }

        KnessetContext context;

        public AddProtocolWindow()
        {
            InitializeComponent();
            OpenFileDialog ofd = new OpenFileDialog { Filter = "XML Filse|*.xml" };
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    context = new KnessetContext();
                    LoadProtocolFile(ofd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    Close(); return;
                }
            }
            else
            {
                Close(); return;
            }
            DataContext = protocol;
        }

        private void LoadProtocolFile(string fileName)
        {
            ProtocolFileParser p = new ProtocolFileParser(fileName);
            protocol = p.Parse(context);
        }

        private void SaveProtocol(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("TO BE DONE!");
        }
    }
}
