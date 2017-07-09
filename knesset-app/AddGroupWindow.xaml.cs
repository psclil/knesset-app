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

namespace knesset_app
{
    /// <summary>
    /// Interaction logic for AddGroupWindow.xaml
    /// </summary>
    public partial class AddGroupWindow : Window
    {
        public AddGroupWindow()
        {
            InitializeComponent();
        }

        private void SaveGroup(object sender, RoutedEventArgs e)
        {
            string grpName = groupNameTxt.Text;
            if (grpName.Length == 0)
            {
                MessageBox.Show("חובה להזין שם לקבוצה");
                groupNameTxt.Focus();
                return;
            }
            HashSet<string> items = new HashSet<string>();
            ParagraphReader reader = new ParagraphReader();
            reader.Read(wordsListTxt.Text, w => { items.Add(w); }, w => { });
            if (items.Count == 0)
            {
                MessageBox.Show("חובה להזין מילים לקבוצה");
                wordsListTxt.SelectAll();
                return;
            }
            else if (items.Count > WordsGroup.MaxItemsInGroup)
            {
                MessageBox.Show(string.Format("לא ניתן להזין יותר מ-{0} מילים בקבוצה", WordsGroup.MaxItemsInGroup));
                return;
            }
            try
            {
                using (KnessetContext context = new KnessetContext())
                {
                    WordsGroup existing = context.WordsGroups.Find(grpName);
                    if (existing != null)
                    {
                        MessageBox.Show("כבר קיימת קבוצה עם שם זה");
                        return;
                    }
                    WordsGroup group = new WordsGroup { g_name = grpName };
                    context.WordsGroups.Add(group);

                    foreach (var wordStr in items)
                    {
                        Word wordObj = context.Words.Find(wordStr);
                        if (wordObj == null)
                        {
                            wordObj = new Word { word = wordStr };
                            context.Words.Add(wordObj);
                        }
                        context.WordInGroups.Add(new WordInGroup { wordsGroup = group, WordObj = wordObj });
                    }
                    context.SaveChanges();
                }
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
