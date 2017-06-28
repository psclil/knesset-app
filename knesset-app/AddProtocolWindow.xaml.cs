using knesset_app.DBEntities;
using Microsoft.Win32;
using System;
using System.Threading;
using System.Windows;

namespace knesset_app
{
    /// <summary>
    /// Interaction logic for AddProtocolWindow.xaml
    /// </summary>
    public partial class AddProtocolWindow : Window
    {

        internal Protocol Protocol { get; set; }

        ProtocolFileParser fileParser;

        KnessetContext context;

        public AddProtocolWindow(string fileName)
        {
            InitializeComponent();
            try
            {
                context = new KnessetContext();
                fileParser = new ProtocolFileParser(fileName);
                Protocol = fileParser.Parse(context);
                DataContext = Protocol;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void SaveProtocol(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            Protocol existing = context.Protocols.Find(Protocol.c_name, Protocol.pr_number);
            if (existing != null && existing != Protocol)
            {
                var choise = MessageBox.Show("דריסת פרוטוקול קיים?", "הפרוטוקול קיים", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.No);
                if (choise == MessageBoxResult.Yes)
                {
                    context.Protocols.Remove(existing);
                    context.Protocols.Add(Protocol);
                }
                else if (choise == MessageBoxResult.No)
                {
                    Close();
                    return;
                }
                else
                {
                    IsEnabled = true;
                    return;
                }
            }
            else
            {
                context.Protocols.Add(Protocol);
            }
            Thread t = new Thread(() =>
            {
                Random r = new Random();
                context.Database.Log = (s) => // show some random data when performing DB access to show the user the app is doing something...
                {
                    if (r.Next(20) == 0)
                        try
                        {
                            Dispatcher.Invoke(() =>
                            {
                                debugArea.Text = s;
                            });
                        }
                        catch (Exception) { }
                };

                try
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    context.Configuration.ValidateOnSaveEnabled = false;

                    Committee c = context.Committees.Find(Protocol.c_name);
                    if (c == null)
                    {
                        c = new Committee { c_name = Protocol.c_name };
                        context.Committees.Add(c);
                    }
                    Protocol.committee = c;
                    context.Protocols.Add(Protocol);

                    context.Persons.AddRange(fileParser.newPersons);
                    context.Persences.AddRange(fileParser.newPresence);
                    context.Invitations.AddRange(fileParser.newInvitations);
                    context.Words.AddRange(fileParser.newWords);
                    context.Paragraphs.AddRange(fileParser.newParagraphs);
                    context.ParagraphWords.AddRange(fileParser.newParagraphWords);

                    int updatedObjects = context.SaveChanges();
                    //MessageBox.Show(string.Format("Saved {0} objects successfully.", updatedObjects), "Saved Objects", MessageBoxButton.OK, MessageBoxImage.Information);
                    Dispatcher.Invoke(() =>
                    {
                        DialogResult = true;
                        Close();
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        IsEnabled = true;
                    });
                    System.IO.File.WriteAllText("exception.txt", ex.ToString());
                    MessageBox.Show(ex.ToString());
                }
            })
            {
                IsBackground = true
            };
            t.Start();
        }
    }
}
