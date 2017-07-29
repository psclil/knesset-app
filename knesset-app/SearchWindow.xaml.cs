using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using knesset_app.DBEntities;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.Entity;
using System.Globalization;

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

            string protocolTitle = string.Empty;
            string selectedProcotocolCommitte = string.Empty;

            //getting the date fields 
            DateTime? fromDate = dpFromDate.SelectedDate;
            DateTime? toDate = dpToDate.SelectedDate;

            List<Protocol> protocols = new List<Protocol>();


            if (cbProtocolTitle.Text != null && string.IsNullOrWhiteSpace(cbProtocolTitle.Text) == false)
            {
                protocolTitle = cbProtocolTitle.Text;
            }

            //commettie field 
            if (cbProtocolCommitte.SelectedValue != null && string.IsNullOrWhiteSpace(cbProtocolCommitte.SelectedValue.ToString()) == false)
            {
                selectedProcotocolCommitte = cbProtocolCommitte.SelectedValue.ToString();
            }


            int PersonIsExist = 1;


            List<Protocol> invitedProtocolList = new List<Protocol>();
            if (string.IsNullOrWhiteSpace(cbInvited.Text) == false)
            {
                string[] split = cbInvited.Text.Split(new char[','], StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < split.Length; i++)
                {
                    split[i] = split[i].Trim();
                }

                IQueryable<Protocol> invitedListQuery = context.Protocols;
                foreach (string temps in split)
                {
                    invitedListQuery = invitedListQuery.Where(x => x.invitations.Any(i => i.pn_name == temps));

                }


                foreach (var prot in invitedListQuery)
                {
                    invitedProtocolList.AddRange(invitedListQuery.Where(x => x.c_name == prot.c_name && x.pr_number == prot.pr_number));
                }



            }

            // invitedProtocolListQuery = context.Protocols.Where(x => x.invitations.Any(i => split.Contains(i.pn_name)));
            //invitedProtocolListQuery = invitedProtocolListQuery.Where(x => x.c_name == prot.c_name && x.pr_number == prot.pr_number);
            /* IQueryable<Protocol> invitedListQuery = context.Protocols.Where(x => x.invitations.Any(i => split.Contains(i.pn_name)));
 
            */



            List<Protocol> presenceProtocolList = new List<Protocol>();
            if (string.IsNullOrWhiteSpace(cbPersence.Text) == false)
            {
                string[] split = cbPersence.Text.Split(new char[','], StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < split.Length; i++)
                {
                    split[i] = split[i].Trim();
                }

            }





            if (PersonIsExist == 1)
            {

                IQueryable<Protocol> relevantProtocols = context.Protocols;
                if (!string.IsNullOrEmpty(selectedProcotocolCommitte))
                    relevantProtocols = relevantProtocols.Where(x => x.c_name == selectedProcotocolCommitte);
                if (!string.IsNullOrEmpty(protocolTitle))
                    relevantProtocols = relevantProtocols.Where(x => x.pr_title == protocolTitle);
                if (fromDate != null)
                    relevantProtocols = relevantProtocols.Where(x => x.pr_date >= fromDate);
                if (toDate != null)
                    relevantProtocols = relevantProtocols.Where(x => x.pr_date <= toDate);


                protocols = (from p in context.Protocols
                             join i in relevantProtocols
                            on new { p.c_name, p.pr_number } equals new { i.c_name, i.pr_number }
                             select p
                          ).ToList().Distinct(new ProtocolsComparer()).ToList();


                // intersecting with invitations :           
                if (invitedProtocolList.Count > 0)
                {
                    protocols = (from p in protocols
                                 join i in invitedProtocolList
                                 on new { p.c_name, p.pr_number } equals new { i.c_name, i.pr_number }
                                 select p
                             ).ToList().Distinct(new ProtocolsComparer()).ToList();


                }


                // intersecting with presence :           
                if (presenceProtocolList.Count > 0)
                {
                    protocols = (from p in protocols
                                 join i in presenceProtocolList
                                     on new { p.c_name, p.pr_number } equals new { i.c_name, i.pr_number }
                                 select p
                             ).ToList().Distinct(new ProtocolsComparer()).ToList();
                }


            }
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
            else if (IsSpeakerTRUE)
            {

                string speakerName = string.Empty;
                selectedProcotocolName = protocolName.Text;
                speakerName = SpkeakerName.Text;
                int paragraphSpekerNum = int.Parse(PgSpeakerNum.Text);
                int paragraphSpekerOffset = int.Parse(PgOffset.Text);



                IQueryable<Protocol> relevantProtocols = context.Protocols.Where(i => i.pr_title.Contains(selectedProcotocolName));
                IQueryable<DBEntities.Paragraph> relevantParagraphs = context.Paragraphs.Where(i => i.pn_name.Contains(speakerName)
                && i.pn_pg_number == paragraphSpekerNum);

                searchedWords = (from p in context.ParagraphWords
                                 join i in relevantProtocols
                                 on new { p.c_name, p.pr_number } equals new { i.c_name, i.pr_number }
                                 where p.pg_offset == paragraphSpekerOffset
                                 select p
                             ).ToList();

                searchedWords = (from p in searchedWords
                                 join i in relevantParagraphs
                                 on new { p.c_name, p.pr_number } equals new { i.c_name, i.pr_number }
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
            string selectedExpression = string.Empty;

            if (tbListExpression.Text != null && string.IsNullOrWhiteSpace(tbListExpression.Text.ToString()) == false)
            {
                selectedExpression = tbListExpression.Text;


                List<ParagraphWord> slecetedExpression = context.ParagraphWords.Where(j => j.word.Contains(selectedExpression)).ToList();


                if (slecetedExpression != null && slecetedExpression.Count > 0)
                {
                    // context.Paragraphs.Include("words").Where(x => x.c_name == protocol.c_name && x.pr_number == protocol.pr_number).ToList();
                    lsResultsExpression.ItemsSource = slecetedExpression;
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


        }


        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }



        protected void SpecializedSoftware_CheckedChanged(object sender, EventArgs e)
        {

        }




        private void wordNum_TextChanged(object sender, EventArgs e)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(wordNum.Text, "^[0-9]*$"))
            {
                wordNum.Text = wordNum.Text.Substring(0, wordNum.Text.Length == 0 ? 0 : wordNum.Text.Length - 1);
                wordNum.CaretIndex = wordNum.Text.Length;
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