using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using knesset_app.DBEntities;
using System.Data.Entity;
using System.Text.RegularExpressions;

namespace knesset_app
{

    public class ProtocolsComparer : IEqualityComparer<Protocol>
    {
        public bool Equals(Protocol x, Protocol y)
        {
            if (x.pr_number == y.pr_number && x.c_name == y.c_name)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetHashCode(Protocol obj)
        {
            return obj.pr_number.GetHashCode();
        }
    }


    /// <summary>
    /// Interaction logic for SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : Window
    {
        int MAX_COMMITTIE = 1000;
        Boolean IsSpeakerTRUE = false;

        KnessetContext context = new KnessetContext();

        public SearchWindow()
        {
            InitializeComponent();
            PopulateComboboxes();
            fillComboboxes();
        }

        private void PopulateComboboxes()
        {
            cbProtocolCommitte.ItemsSource = context.Committees.Take(MAX_COMMITTIE).ToList();
            cbProtocolCommitte.DisplayMemberPath = "c_name";
            cbProtocolCommitte.SelectedValuePath = "c_name";
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
            dpFromDate.SelectedDate = dpToDate.SelectedDate = null;

            PopulateComboboxes();
            fillComboboxes();
            cbInvited.Text = null;
            cbPersence.Text = null;
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
                selectedProcotocolName = protocolName.Text;
                int selectedParagraphNum = int.Parse(paragraphNum.Text);
                int selectedWordNum = int.Parse(wordNum.Text);


                IQueryable<Protocol> relevantProtocols = context.Protocols.Where(i => i.pr_title.Contains(selectedProcotocolName));
                searchedWords = (from p in context.ParagraphWords
                                 join i in relevantProtocols
                                 on new { p.c_name, p.pr_number } equals new { i.c_name, i.pr_number }
                                 where p.pg_number == selectedParagraphNum && p.word_number == selectedWordNum
                                 select p
                             ).ToList();

            }

            // serach by speaker
            else
            {

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
                                 && r.pn_name.Contains(speakerName)
                                 && r.pn_pg_number == paragraphSpekerNum
                                 select p
                             ).ToList();
            }



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





        private void fillComboboxes()
        {

            dpListExpression.ItemsSource = context.Phrases.Take(MAX_COMMITTIE).ToList();
            dpListExpression.DisplayMemberPath = "phrase";
            dpListExpression.SelectedValuePath = "phrase";


        }


        private void btnSerach_Expression(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(dpListExpression.Text))
            {
                MessageBox.Show("חובה להכניס ביטוי לחיפוש");
                return;
            }
            string selectedExpression = dpListExpression.Text;
            string[] split = selectedExpression.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < split.Length; i++)
            {
                split[i] = split[i].Trim();
            }

            if (split.Length >= 6)
            {
                MessageBox.Show("יותר מידי מילים בביטוי לחיפוש");
                return;
            }
            string firstWord = split[0];
            IQueryable<ParagraphWord> selecetedExpression = context.ParagraphWords.Where(x => (x.word == firstWord));

            for (int j = 1; j < split.Length; j++)
            {
                string temp = split[j];
                selecetedExpression = selecetedExpression.Where(x => context.ParagraphWords.Any(i => (
                i.word == temp
                &&
                x.pr_number == i.pr_number
                &&
                x.pg_number == i.pg_number
                &&
                x.word_number + j == i.word_number)));

            }

            List<ParagraphWord> results = selecetedExpression.ToList();

            if (results.Count > 0)
            {
                // context.Paragraphs.Include("words").Where(x => x.c_name == protocol.c_name && x.pr_number == protocol.pr_number).ToList();
                //lsResultsExpression.ItemsSource = ProtocolExpression;
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

        private void tbListExpression_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}