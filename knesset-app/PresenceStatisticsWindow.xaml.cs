using System;
using System.Windows;

namespace knesset_app
{
    /// <summary>
    /// Interaction logic for PresenceStatisticsWindow.xaml
    /// </summary>
    public partial class PresenceStatisticsWindow : Window
    {
        PresenceStatisticsData data; // <---- this object is used for data binding and is responsible for all the statistical calculations

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
