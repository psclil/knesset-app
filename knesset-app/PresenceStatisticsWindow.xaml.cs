using knesset_app.DBEntities;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace knesset_app
{
    /// <summary>
    /// Interaction logic for PresenceStatisticsWindow.xaml
    /// </summary>
    public partial class PresenceStatisticsWindow : Window
    {
        PresenceStatisticsData data;

        public PresenceStatisticsWindow()
        {
            InitializeComponent();
        }

        private void InitData(object sender, RoutedEventArgs e)
        {
            try
            {
                data = new PresenceStatisticsData();
                DataContext = data;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
