﻿using System.Windows;

namespace knesset_app
{
    /// <summary>
    /// Interaction logic for WordFrequenciesWindow.xaml
    /// </summary>
    public partial class WordFrequenciesWindow : Window
    {
        public WordFrequenciesWindow()
        {
            // the logic and DB access for this window is over at WordFrequenciesData
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            (DataContext as WordFrequenciesData).UpdateResults();
        }
    }
}
