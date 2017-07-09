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
using knesset_app.DBEntities;
using Microsoft.Win32;
using System.IO;

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
                UIData = new CreateIndexData();
                DataContext = UIData;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
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
            if (sfd.ShowDialog().GetValueOrDefault())
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(sfd.FileName, false, Encoding.Unicode))
                    {
                        using (KnessetContext context = new KnessetContext())
                        {
                            string _lastWord = null;
                            Action<ParagraphWord> writePW = (ParagraphWord pw) =>
                            {
                                if (pw.word != _lastWord)
                                {
                                    _lastWord = pw.word;
                                    sw.WriteLine(pw.word);
                                }
                                sw.WriteLine("\t{0} [{1}] Paragraph {2} Word {3}", pw.c_name, pw.pr_number, pw.pg_number, pw.word_number);
                                sw.WriteLine("\t{0} [{1}] Speaker {2} Paragraph {3} Offset {4}", pw.c_name, pw.pr_number, pw.paragraph.pn_name, pw.paragraph.pn_pg_number, pw.pg_offset);
                            };
                            IQueryable<ParagraphWord> wordsListQuery = context.ParagraphWords.Include("paragraph");
                            if (selectedGroups.Count > 0)
                            {
                                string[] selectedGroupsWords = distinctFromGroups(selectedGroups);
                                wordsListQuery = wordsListQuery.Where(x => selectedGroupsWords.Contains(x.word));
                            }
                            if (selectedProtocols.Count == 0)
                                foreach (var pw in wordsListQuery.OrderBy(x => x.word))
                                    writePW(pw);
                            else
                            {
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
                MessageBox.Show(sfd.FileName, "האינדקס נשמר", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private string[] distinctFromGroups(List<WordsGroup> selectedGroups)
        {
            List<string> allWords = new List<string>();
            foreach (var grp in selectedGroups)
            {
                allWords.AddRange(grp.items.Select(x => x.word));
            }
            return allWords.Distinct().ToArray();
        }
    }
}
