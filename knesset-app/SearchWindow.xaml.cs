using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using knesset_app.DBEntities;
using System.Data.Entity;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Media;

namespace knesset_app
{

    /// <summary>
    /// Interaction logic for SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : Window
    {
        int MAX_COMBOXLIST = 1000;
        Boolean IsSpeakerTRUE = false;

        KnessetContext context = new KnessetContext();

        public SearchWindow()
        {
            InitializeComponent();
            PopulateComboboxes();
            dpListExpression.PreviewMouseRightButtonDown += OnPreviewMouseRightButtonDown;
        }

        private void PopulateComboboxes()
        {
            cbProtocolCommitte.ItemsSource = context.Committees.Take(MAX_COMBOXLIST).ToList();
            cbProtocolCommitte.DisplayMemberPath = "c_name";
            cbProtocolCommitte.SelectedValuePath = "c_name";

            dpListExpression.ItemsSource = context.Phrases.Take(MAX_COMBOXLIST).ToList();
            dpListExpression.DisplayMemberPath = "phrase";
            dpListExpression.SelectedValuePath = "phrase";
        }



        private void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var comboBoxItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (comboBoxItem == null) return;
            comboBoxItem.IsSelected = true;
            e.Handled = true;
        }

        private ComboBoxItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is ComboBoxItem))
                source = VisualTreeHelper.GetParent(source);

            return source as ComboBoxItem;
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            dpListExpression.Items.Remove(dpListExpression.SelectedItem);
        }




        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // ... Get TabControl reference.
            TabControl item = sender as TabControl;
            // ... Set Title to selected tab header.
            var selected = item.SelectedItem as TabItem;
            this.Title = selected.Header.ToString();
            TabItem ti = (TabItem)item.SelectedItem;

            if (ti.Name == "tiSearchCommitties")
            {

            }
        }





        private void btnClearFields_Click(object sender, RoutedEventArgs e)
        {
            cbProtocolTitle.Text = string.Empty;
            cbProtocolCommitte.Text = string.Empty;
            cbProtocolCommitte.SelectedIndex = -1;
            cbInvited.Text = null;
            cbPersence.Text = null;
            dpFromDate.SelectedDate = dpToDate.SelectedDate = null;
            PopulateComboboxes();

            protocolName.Text = string.Empty;
            PgOffset.Text = string.Empty;
            SpkeakerName.Text = string.Empty;
            PgOffset.Text = null;
            PgSpeakerNum.Text = null;
            paragraphNum.Text = null;
            wordNum.Text = null;

            dpListExpression.Text = string.Empty;
            dpListExpression.SelectedIndex = -1;

            lsResultsExpression.ItemsSource = string.Empty;
            lstResults.ItemsSource = string.Empty;
            lstResultsBackwardSearch.ItemsSource = string.Empty;
        }





        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            IQueryable<Protocol> relevantProtocols = context.Protocols;

            string protocolTitle = string.IsNullOrEmpty(cbProtocolTitle.Text) ? string.Empty : cbProtocolTitle.Text;
            string selectedProcotocolCommitte = string.Empty;

            //getting the date fields 
            DateTime? fromDate = dpFromDate.SelectedDate;
            DateTime? toDate = dpToDate.SelectedDate;

            //commettie field 
            if (cbProtocolCommitte.SelectedValue != null && string.IsNullOrWhiteSpace(cbProtocolCommitte.SelectedValue.ToString()) == false)
            {
                selectedProcotocolCommitte = cbProtocolCommitte.SelectedValue.ToString();
            }

            if (!string.IsNullOrEmpty(selectedProcotocolCommitte))
                relevantProtocols = relevantProtocols.Where(x => x.c_name == selectedProcotocolCommitte);
            if (!string.IsNullOrEmpty(protocolTitle))
                relevantProtocols = relevantProtocols.Where(x => x.pr_title.Contains(protocolTitle));
            if (fromDate.HasValue)
                relevantProtocols = relevantProtocols.Where(x => x.pr_date >= fromDate);
            if (toDate.HasValue)
                relevantProtocols = relevantProtocols.Where(x => x.pr_date <= toDate);



            //invidted field
            char[] nameListSplit = new char[] { ',' };

            if (!string.IsNullOrWhiteSpace(cbInvited.Text))
            {
                string[] split = cbInvited.Text.Split(nameListSplit, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < split.Length; i++)
                {
                    split[i] = split[i].Trim();
                }

                foreach (string temps in split)
                {
                    relevantProtocols = relevantProtocols.Where(x => x.invitations.Any(i => i.pn_name == temps));
                }
            }



            //Persences field
            if (string.IsNullOrWhiteSpace(cbPersence.Text) == false)
            {
                string[] split = cbPersence.Text.Split(nameListSplit, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < split.Length; i++)
                {
                    split[i] = split[i].Trim();
                }

                foreach (string temps in split)
                {
                    relevantProtocols = relevantProtocols.Where(x => x.persence.Any(i => i.pn_name == temps));
                }
            }




            // display results
            List<Protocol> protocols = relevantProtocols.ToList();

            if (protocols != null && protocols.Count > 0)
            {
                lstResults.ItemsSource = protocols;
            }
            else
            {
                List<Protocol> messages = new List<Protocol>();
                Protocol message = new Protocol();
                message.pr_title = "לא נמצאו תוצאות";
                messages.Add(message);
                lstResults.ItemsSource = messages;
            }
        }






        private void btnSearch_Backwards(object sender, RoutedEventArgs e)
        {
            List<ParagraphWord> searchedWords = new List<ParagraphWord>();
            string selectedProcotocolName = string.Empty;

            if (!IsSpeakerTRUE)
            {
               
                    if (string.IsNullOrWhiteSpace(protocolName.Text) || string.IsNullOrWhiteSpace(paragraphNum.Text) || string.IsNullOrWhiteSpace(wordNum.Text))
                    {
                        MessageBox.Show("חובה למלא את כל השדות");
                        return;
                    }
                selectedProcotocolName = protocolName.Text;
                int selectedParagraphNum = int.Parse(paragraphNum.Text);
                int selectedWordNum = int.Parse(wordNum.Text);

                IQueryable<Protocol> relevantProtocols = context.Protocols;
                searchedWords = (from p in context.ParagraphWords
                                 join i in relevantProtocols
                                 on new { p.c_name, p.pr_number } equals new { i.c_name, i.pr_number }
                                 where p.pg_number == selectedParagraphNum
                                 &&
                                 p.word_number == selectedWordNum
                                 &&
                                 i.pr_title.Contains(selectedProcotocolName)
                                 select p
                             ).ToList();

            }

            // serach by speaker
            else
            {
                if (string.IsNullOrWhiteSpace(protocolName.Text) || string.IsNullOrWhiteSpace(SpkeakerName.Text) 
                    || string.IsNullOrWhiteSpace(PgSpeakerNum.Text)|| string.IsNullOrWhiteSpace(PgOffset.Text)) 
                {
                    MessageBox.Show("חובה למלא את כל השדות");
                    return;
                }
                string speakerName = string.Empty;
                selectedProcotocolName = protocolName.Text;
                speakerName = SpkeakerName.Text;
                int paragraphSpekerNum = int.Parse(PgSpeakerNum.Text);
                int paragraphSpekerOffset = int.Parse(PgOffset.Text);



                IQueryable<Protocol> relevantProtocols = context.Protocols;
                IQueryable<Paragraph> relevantParagraphs = context.Paragraphs;

                searchedWords = (from p in context.ParagraphWords
                                 join i in relevantProtocols
                                 on new { p.c_name, p.pr_number } equals new { i.c_name, i.pr_number }
                                 join r in relevantParagraphs
                                 on new { p.c_name, p.pr_number, p.pg_number } equals new { r.c_name, r.pr_number, r.pg_number }
                                 where p.pg_offset == paragraphSpekerOffset
                                 &&
                                 i.pr_title.Contains(selectedProcotocolName)
                                 &&
                                 r.pn_name.Contains(speakerName)
                                 &&
                                 r.pn_pg_number == paragraphSpekerNum
                                 select p
                             ).ToList();
            }


            // display results
            if (searchedWords.Count > 0)
            {
                lstResultsBackwardSearch.ItemsSource = searchedWords;
            }
            else
            {
                List<ParagraphWord> messages = new List<ParagraphWord>();
                ParagraphWord message = new ParagraphWord();
                message.c_name = "לא נמצאו תוצאות";
                message.word = "";
                messages.Add(message);
                lstResultsBackwardSearch.ItemsSource = messages;
            }

        }






        //private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        //{
        //    var menuItem = (MenuItem)sender;
        //    var ctxMenu = (ContextMenu)menuItem.Parent;
        //    var comboBoxItem = (ComboBoxItem)ctxMenu.PlacementTarget;
        //}


        private void btnSerach_Expression(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(dpListExpression.Text))
            {
                MessageBox.Show("חובה להכניס ביטוי לחיפוש");
                return;
            }
            char[] nameListSplit = new char[] { ' ' };

            string selectedExpression = dpListExpression.Text;
            ParagraphReader paragraphReader = new ParagraphReader();
            List<string> searchWords = paragraphReader.ReadWords(selectedExpression);

            if (searchWords.Count >= 6)
            {
                MessageBox.Show("יותר מידי מילים בביטוי לחיפוש");
                return;
            }
            string firstWord = searchWords[0];
            IQueryable<ParagraphWord> selecetedExpression = context.ParagraphWords.Where(x => (x.word == firstWord));

            for (int j = 1; j < searchWords.Count; j++)
            {
                // for technical reasons we need to save values used in the query expressions as simple variables
                // and not access them through arrays, etc...
                // we alsp need to copy values that might change until the query is executed.
                string temp = searchWords[j];
                int tempI = j;
                selecetedExpression = selecetedExpression.Join(context.ParagraphWords,
                    x => new { x.c_name, x.pr_number, x.pg_number, word_number = x.word_number + tempI, word = temp },
                    y => new { y.c_name, y.pr_number, y.pg_number, y.word_number, y.word },
                    (x, y) => x);
            }

            var resultsRaw = selecetedExpression.Include("paragraph").Include("paragraph.words").ToList();
            var results = resultsRaw.Select(x => new ParagraphMatch(x.paragraph, x, searchWords)).ToList();

            if (results.Count > 0)
            {
                lsResultsExpression.ItemsSource = results;
            }
            else
            {
                List<Protocol> messages = new List<Protocol>();
                Protocol message = new Protocol();
                message.pr_title = "לא נמצאו תוצאות";
                messages.Add(message);
                lsResultsExpression.ItemsSource = messages;
            }
        }


        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }



        protected void SpecializedSoftware_CheckedChanged(object sender, EventArgs e)
        {

        }




        private void numericTxt_Change(object sender, EventArgs e)
        {
            if (sender == null || !(sender is TextBox)) return;
            TextBox textBox = sender as TextBox;
            if (!Regex.IsMatch(textBox.Text, "^[0-9]*$"))
            {
                textBox.Text = Regex.Replace(textBox.Text, "[^0-9]", "");
            }
        }


        private void openChosenProtocol(object sender, SelectionChangedEventArgs e)
        {
            if (lstResults.SelectedIndex == -1) return;
            ProtocolDisplayWindow chosenP = new ProtocolDisplayWindow(lstResults.SelectedItem as Protocol);
            chosenP.ShowDialog();
            lstResults.SelectedIndex = -1;
        }




        private void CheckBox_Changes(object sender, RoutedEventArgs e)
        {
            if (checkSpeakerBox.IsChecked == false)
            {
                IsSpeakerTRUE = false;
                SpeakerSearch.Visibility = Visibility.Hidden;
                WordNumSearch.Visibility = Visibility.Visible;
            }
            else
            {
                IsSpeakerTRUE = true;
                WordNumSearch.Visibility = Visibility.Hidden;
                SpeakerSearch.Visibility = Visibility.Visible;
            }

        }

        private void dpListExpression_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (dpListExpression.SelectedIndex >= 0)
            //{

            //    dpListExpression.Items.RemoveAt(dpListExpression.SelectedIndex);
            //    return;
            //}

        }
    }
}