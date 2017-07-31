using System;
using System.Collections.Generic;
using System.Windows;
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
            // first do validations that do not reqire db access

            string grpName = groupNameTxt.Text;
            if (grpName.Length == 0)
            {
                MessageBox.Show("חובה להזין שם לקבוצה");
                groupNameTxt.Focus();
                return;
            }
            ParagraphReader reader = new ParagraphReader(); // use paragraph read to remove all "non-word" chars and split into words
            HashSet<string> items = new HashSet<string>(reader.ReadWords(wordsListTxt.Text));
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
                    // now do validations that do reqire db access
                    WordsGroup existing = context.WordsGroups.Find(grpName);
                    if (existing != null)
                    {
                        MessageBox.Show("כבר קיימת קבוצה עם שם זה");
                        return;
                    }

                    // input is OK, save the new group, if a word is not in the words relation add it
                    // (we might define groups before loading protocols)
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
                    context.SaveChanges(); // commit all changes to DB
                }
                DialogResult = true; // can be used by parent window - marks success
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
