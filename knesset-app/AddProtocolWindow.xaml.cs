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

        KnessetContext context;
        ProtocolFileParser fileParser;

        public AddProtocolWindow()
        {
            InitializeComponent();
            OpenFileDialog ofd = new OpenFileDialog { Filter = "XML Filse|*.xml" };
            if (ofd.ShowDialog() == true)
            {
                context = new KnessetContext();
                Random r = new Random();
                context.Database.Log = (s) =>
                {
                    if (r.Next(20) == 4) try
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
                    fileParser = new ProtocolFileParser(ofd.FileName);
                    Protocol = fileParser.Parse(context);
                    DataContext = Protocol;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            else
            {
                Close();
            }
        }

        private void SaveProtocol(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            Committee c = context.Committees.Find(Protocol.c_name);
            if (c == null)
            {
                c = new Committee { c_name = Protocol.c_name };
                context.Committees.Add(c);
            }
            Protocol.committee = c;
            context.Protocols.Add(Protocol);
            Thread t = new Thread(() =>
            {
                try
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    context.Configuration.ValidateOnSaveEnabled = false;

                    context.Persons.AddRange(fileParser.newPersons);
                    context.Persences.AddRange(fileParser.newPresence);
                    context.Invitations.AddRange(fileParser.newInvitations);
                    context.Words.AddRange(fileParser.newWords);
                    context.Paragraphs.AddRange(fileParser.newParagraphs);
                    context.ParagraphWords.AddRange(fileParser.newParagraphWords);

                    int updatedObjects = context.SaveChanges();
                    MessageBox.Show(string.Format("Saved {0} objects successfully.", updatedObjects), "Saved Objects", MessageBoxButton.OK, MessageBoxImage.Information);

                    Dispatcher.Invoke(() => { Close(); });
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
