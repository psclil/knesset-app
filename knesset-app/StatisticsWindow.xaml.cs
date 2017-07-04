using System.Windows;

namespace knesset_app
{
    /// <summary>
    /// Interaction logic for StatisticsWindow.xaml
    /// </summary>
    public partial class StatisticsWindow : Window
    {
        public StatisticsWindow()
        {
            InitializeComponent();
        }

        private void InitStatistics(object sender, RoutedEventArgs e)
        {

        }

        private void WordFrequencies(object sender, RoutedEventArgs e)
        {
            WordFrequenciesWindow freqWindow = new WordFrequenciesWindow();
            freqWindow.ShowDialog();
        }

        private void Presence(object sender, RoutedEventArgs e)
        {
            PresenceStatisticsWindow presenceWindow = new PresenceStatisticsWindow();
            presenceWindow.ShowDialog();
        }
    }
}
