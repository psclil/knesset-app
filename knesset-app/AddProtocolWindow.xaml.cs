using knesset_app.DBEntities;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace knesset_app
{
    /// <summary>
    /// Interaction logic for AddProtocolWindow.xaml
    /// </summary>
    public sealed partial class AddProtocolWindow : Window, IDisposable
    {

        public static bool AutoSaveAll { get; set; } = false;

        internal Protocol Protocol { get; set; }

        ProtocolFileParser fileParser;

        KnessetContext context;

        public AddProtocolWindow(string fileName)
        {
            InitializeComponent();
            try
            {
                // Load the protocol file because the window is shown
                // because the protocol object will be binded to the ui presented to the user.
                // we're using only one context object and not disposing each time because another
                // context would consider fetched db entities as new and try to insert them.
                // the down side is that the app memory usage will not be optimal when importing many protocols.
                context = new KnessetContext();
                fileParser = new ProtocolFileParser(fileName);
                Protocol = fileParser.Parse(context);
                DataContext = Protocol; // Databind the UI to the protocol object.
            }
            catch (Exception ex)
            {
                // if there was a problem show a message to the user, protocol will be null and the user
                // of this window will not show it.
                MessageBox.Show(ex.ToString(), fileName, MessageBoxButton.OK, MessageBoxImage.Asterisk);

            }
        }

        private void SaveProtocol(object sender, RoutedEventArgs e)
        {
            IsEnabled = false; // disable the ui while saving the protocol so that the user does not change anything in the meanwhile.
            if (sender != null)
                AutoSaveAll = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl); // when batch importing sometimes it's easyer if save run automatically.
            Protocol existing = context.Protocols.Find(Protocol.c_name, Protocol.pr_number); // check to see if this protocol alerady exists in the db
            if (existing != null && existing != Protocol)
            {
                var choise = !AutoSaveAll ? MessageBox.Show(
                    "דריסת פרוטוקול קיים?",
                    "הפרוטוקול קיים",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.No) : MessageBoxResult.No;
                if (choise == MessageBoxResult.Yes)
                {
                    // delete the existing protocol, because of foreign keys all relevant data will also be deleted
                    context.Protocols.Remove(existing);
                    // and swap with the new protocol
                    context.Protocols.Add(Protocol);
                }
                else if (choise == MessageBoxResult.No)
                {
                    // not replacing so just close the window
                    Close();
                    return;
                }
                else
                {
                    // cancel, do nothing
                    IsEnabled = true;
                    return;
                }
            }
            Thread t = new Thread(() => // declare the save logic to run on a new thread in order not to hang the UI thread.
            {

                // show some of the data logged by the contexts performing DB access
                // in order to show the user the app is doing something and is not stuck.
                Random rndGenerator = new Random();
                context.Database.Log = (logMessage) =>
                {
                    if (rndGenerator.Next(20) == 0) // one of 20 messages on avg. will be shown to the user for a small amount of time.
                    {
                        try
                        {
                            Dispatcher.Invoke(() => // because we're on another thread we need to communicate to the UI thread in order to update parts of it.
                            {
                                debugArea.Text = logMessage; // display the message.
                            });
                        }
                        // this might not work if the user is closing the window, etc... try-catch to prevent crush.
                        catch (Exception) { }
                    }
                };

                try
                {
                    context.Configuration.AutoDetectChangesEnabled = false; // this is a known 'hack' to
                    context.Configuration.ValidateOnSaveEnabled = false;   // improve SaveChanges() performence.

                    Committee c = context.Committees.Find(Protocol.c_name); // add a new Committee object if needed.
                    if (c == null)
                    {
                        c = new Committee { c_name = Protocol.c_name };
                        context.Committees.Add(c);
                    }
                    Protocol.committee = c; // link the protocol to the committee object

                    // Add all objects to the context local store:

                    context.Protocols.Add(Protocol);

                    context.Persons.AddRange(fileParser.newPersons);
                    context.Persence.AddRange(fileParser.newPresence);
                    context.Invitations.AddRange(fileParser.newInvitations);
                    context.Words.AddRange(fileParser.newWords);
                    context.Paragraphs.AddRange(fileParser.newParagraphs);
                    context.ParagraphWords.AddRange(fileParser.newParagraphWords);

                    // save all objects
                    // no worry, this will be done in a transaction so no partial saves will happen...
                    int updatedObjects = context.SaveChanges();

                    // no exception, we can wrap up...
                    Dispatcher.Invoke(() =>
                    {
                        // because we're on another thread we need to communicate to the UI thread in order to close the window, etc...
                        DialogResult = true;
                        Close();
                    });
                }
                catch (Exception ex)
                {
                    // if there's an exception show the user a message and go back to the save before "Save" was clicked.
                    Dispatcher.Invoke(() =>
                    {
                        IsEnabled = true;
                    });
                    MessageBox.Show(ex.ToString());
                }
            })
            {
                IsBackground = true // if app closes also kill the thread.
            };
            t.Start(); // start the save thread
        }

        public void Dispose()
        {
            // we need to dispose the context because we're using it globally in the class and not with a using clause
            ((IDisposable)context).Dispose();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Protocol.pr_number > 0 && AutoSaveAll)
            {
                SaveProtocol(null, null);
            }
        }
    }
}
