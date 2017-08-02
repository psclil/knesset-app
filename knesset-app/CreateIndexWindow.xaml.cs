using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using knesset_app.DBEntities;
using Microsoft.Win32;
using System.IO;
using System.Windows.Input;

namespace knesset_app
{
    /// <summary>
    /// Interaction logic for CreateIndexWindow.xaml
    /// </summary>
    public partial class CreateIndexWindow : Window
    {
        public CreateIndexWindow()
        {
            InitializeComponent();
        }

        private CreateIndexData UIData { get; set; }

        private void InitData(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                UIData = new CreateIndexData();
                DataContext = UIData;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void AddGroup(object sender, RoutedEventArgs e)
        {
            AddGroupWindow w = new AddGroupWindow();
            w.ShowDialog();
            if (w.DialogResult.GetValueOrDefault())
                UIData.UpdateData(false, true);
        }

        private void DeleteGroup(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GroupsBox.SelectedItems.Count == 0) return;
                using (KnessetContext context = new KnessetContext())
                {
                    foreach (var wg in GroupsBox.SelectedItems.OfType<WordsGroup>())
                    {
                        context.WordsGroups.Attach(wg);
                        context.WordsGroups.Remove(wg);
                    }
                    context.SaveChanges();
                }
                UIData.UpdateData(false, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void CreateIndex(object sender, RoutedEventArgs e)
        {
            var selectedGroups = GroupsBox.SelectedItems.OfType<WordsGroup>().ToList();
            var selectedProtocols = ProtocolsBox.SelectedItems.OfType<Protocol>().ToList();
            if (selectedGroups.Count == 0 && selectedProtocols.Count == 0)
            {
                MessageBox.Show("חובה לבחור פרוטוקולים או קבוצות");
                return;
            }
            SaveFileDialog sfd = new SaveFileDialog { Filter = "Text Index File | *.txt", AddExtension = true };
            if (sfd.ShowDialog().GetValueOrDefault()) // if user has chosen dest file and approved
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;

                    using (StreamWriter sw = new StreamWriter(sfd.FileName, false, Encoding.Unicode)) // create a write stream to the file
                    {
                        using (KnessetContext context = new KnessetContext()) // create a db connection
                        {
                            string _lastWord = null;
                            Action<ParagraphWord> writePW = (ParagraphWord pw) => // a lambda func to write a single word to the index
                            {
                                if (pw.word != _lastWord)
                                {
                                    _lastWord = pw.word;
                                    sw.WriteLine(pw.word);
                                }
                                sw.WriteLine("\t{0} [{1}] Paragraph {2} Word {3}", pw.c_name, pw.pr_number, pw.pg_number, pw.word_number);
                                sw.WriteLine("\t{0} [{1}] Speaker {2} Paragraph {3} Offset {4}", pw.c_name, pw.pr_number, pw.paragraph.pn_name, pw.paragraph.pn_pg_number, pw.pg_offset);
                            };
                            IQueryable<ParagraphWord> wordsListQuery = context.ParagraphWords.Include("paragraph"); // a query variable to fetch words with their paragraph info
                            if (selectedGroups.Count > 0)
                            {
                                string[] selectedGroupsWords = distinctFromGroups(selectedGroups);
                                wordsListQuery = wordsListQuery.Where(x => selectedGroupsWords.Contains(x.word)); // filter by word group[s]
                            }
                            if (selectedProtocols.Count == 0)
                                foreach (var pw in wordsListQuery.OrderBy(x => x.word)) // if not filtering by protocol just write all the results
                                    writePW(pw);
                            else
                            {
                                // else fetch for each protocol seperatly (we do not have a way to create an OR condition as large as we want)
                                // we prefetch all the data because we need to sort in memoty by the word
                                // this has OK performence, if it wouldn't we would run a custom SQL query.
                                List<ParagraphWord> wordsList = new List<ParagraphWord>();
                                foreach (var protocol in selectedProtocols)
                                    wordsList.AddRange(wordsListQuery.Where(x => x.c_name == protocol.c_name && x.pr_number == protocol.pr_number));
                                foreach (var pw in wordsList.OrderBy(x => x.word))
                                    writePW(pw);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    return;
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
                // display a success message to the user
                MessageBox.Show(sfd.FileName, "האינדקס נשמר", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private string[] distinctFromGroups(List<WordsGroup> selectedGroups)
        {
            // return a union of the words in all the groups 
            List<string> allWords = new List<string>();
            foreach (var grp in selectedGroups)
            {
                allWords.AddRange(grp.items.Select(x => x.word));
            }
            return allWords.Distinct().ToArray();
        }
    }
}
