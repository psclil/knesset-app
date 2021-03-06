﻿using System.Windows;
using Microsoft.Win32;
using System.Windows.Input;
using System.Diagnostics;
using System;

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
            // let the user batch load protocols with auto saving if he presses Ctrl while clicking the add protocol window
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                AddProtocolWindow.AutoSaveAll = true;
            // lets the user select multiple xml files, then for each of them show the add protocol window to edit/save/cancel...
            OpenFileDialog ofd = new OpenFileDialog { Filter = "XML Filse|*.xml", Multiselect = true };
            if (ofd.ShowDialog() == true)
            {
                foreach (string fname in ofd.FileNames)
                {
                    using (
                        var ap = new AddProtocolWindow(fname)
                        {
                            Left = Left, // align to the current window each time
                            Top = Top // instead of random placement
                        })
                    {
                        if (ap.Protocol != null)
                            ap.ShowDialog(); // only show the window if protocol parsine succeded
                    }
                }
            }
        }

        private void ShowStatistics(object sender, RoutedEventArgs e)
        {
            StatisticsWindow statsWin = new StatisticsWindow();
            statsWin.ShowDialog();
        }


        private void Search(object sender, RoutedEventArgs e)
        {
            SearchWindow Win = new SearchWindow();
            Win.ShowDialog();
        }



        private void CreateIndex(object sender, RoutedEventArgs e)
        {
            CreateIndexWindow createWin = new CreateIndexWindow();
            createWin.ShowDialog();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                // make sure no thread hangs, etc... kill the process.
                Process currProcess = Process.GetCurrentProcess();
                currProcess.Kill();
            }
            catch (Exception) { }
        }
    }
}