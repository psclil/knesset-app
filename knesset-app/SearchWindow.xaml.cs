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
            if (x.pr_number == y.pr_number)
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

        KnessetContext context = new KnessetContext();

        public SearchWindow()
        {
            InitializeComponent();

            PopulateComboboxes();
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

            cbInvited.Text = null;
            cbPersence.Text = null;
        }

    



        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

            string protocolTitle = string.Empty;
            string selectedProcotocolCommitte = string.Empty;
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



            //getting the date fields 
            DateTime? fromDate = dpFromDate.SelectedDate;
            DateTime? toDate = dpToDate.SelectedDate;



            int PersonIsExist = 1;

            List<Invitation> lstinvt = new List<Invitation>();
            List<Invitation> lstinvted = new List<Invitation>();
            if (string.IsNullOrWhiteSpace(cbInvited.Text) == false)
            {
                string[] split = cbInvited.Text.Split(',');
                for (int i = 0; i < split.Length; i++)
                {
                    split[i] = split[i].Trim();
                }
                string temp = split[0];
                lstinvted = context.Invitations.Where(p => p.pn_name.Equals(temp)).ToList();

                for (int i = 1; i <= split.Length; i++)
                {
                    string s = split[i - 1];
                    lstinvt = context.Invitations.Where(j => j.pn_name.Equals(s)).ToList();
                    if (lstinvt.Count == 0)
                    {
                        PersonIsExist = 0;
                        break;
                    }
                    else
                    {
                        lstinvted = (from p in lstinvted
                                     join j in lstinvt
                                       on p.pr_number equals j.pr_number
                                     select new Invitation
                                     {
                                         pr_number = p.pr_number,
                                         pn_name = p.pn_name
                                     }
                                ).ToList();
                        if (lstinvted.Count == 0)
                        {
                            PersonIsExist = 0;
                            break;
                        }


                    }
                }
            }

            List<Presence> lstPrese = new List<Presence>();
            List<Presence> lstPersence = new List<Presence>();

            if (PersonIsExist == 1)
            {


                if (string.IsNullOrWhiteSpace(cbPersence.Text) == false)
                {
                    string[] split = cbPersence.Text.Split(',');
                    foreach (string s in split)
                    {
                        s.Trim();
                    }
                    string temp = split[0];
                    lstPersence = context.Persence.Where(p => p.pn_name == temp).ToList();
                    for (int i = 1; i <= split.Length; i++)
                    {
                        string s = split[i - 1];
                        lstPrese = context.Persence.Where(j => j.pn_name == s).ToList();
                        if (lstPrese.Count == 0)
                        {
                            PersonIsExist = 0;
                            break;
                        }
                        else
                        {
                            lstPersence = (from p in lstPersence
                                           join j in lstPrese
                                          on p.pr_number equals j.pr_number
                                           select new Presence
                                           {
                                               pr_number = p.pr_number,
                                               pn_name = p.pn_name
                                           }
                                    ).ToList();
                            if (lstPersence.Count == 0)
                            {
                                PersonIsExist = 0;
                                break;
                            }


                        }
                    }
                }

            }



            if (PersonIsExist == 1)
            {
                if (
                dpFromDate != null
                || toDate != null
                || string.IsNullOrWhiteSpace(selectedProcotocolCommitte) == false
                || string.IsNullOrWhiteSpace(protocolTitle) == false
                || lstinvted.Count > 0
                || lstPersence.Count > 0
                || lstPersence.Count > 0
                )
                {
                    protocols = context.Protocols.Where(p =>
                   (string.IsNullOrEmpty(selectedProcotocolCommitte) || p.c_name.Contains(selectedProcotocolCommitte))
                   &&
                   (string.IsNullOrEmpty(protocolTitle) || p.pr_title.Contains(protocolTitle))
                   &&
                   (fromDate == null || p.pr_date >= fromDate)
                   &&
                   (toDate == null || p.pr_date <= toDate)
                   ).ToList();


                    // intersecting with invitations :           
                    if (lstinvted.Count > 0)
                    {

                        protocols = (from p in protocols
                                     join i in lstinvted
                                     on p.pr_number equals i.pr_number
                                     //   where p.c_name == i.c_name
                                     select new Protocol
                                     {
                                         pr_number = p.pr_number,
                                         pr_title = p.pr_title,
                                         pr_date = p.pr_date
                                     }
                                     ).ToList().Distinct(new ProtocolsComparer()).ToList();
                    }


                    // intersecting with presence :           
                    if (lstPersence.Count > 0)
                    {

                        protocols = (from p in protocols
                                     join rp in lstPersence
                                     on p.pr_number equals rp.pr_number
                                     //   where p.c_name == rp.c_name
                                     select new Protocol
                                     {
                                         pr_number = p.pr_number,
                                         pr_title = p.pr_title,
                                         pr_date = p.pr_date
                                     }
                                     ).ToList().Distinct(new ProtocolsComparer()).ToList();
                    }

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

            Boolean IsSpeakerTRUE = false;
            List<ParagraphWord> searchedWords = new List<ParagraphWord>();
            string selectedProcotocolName = string.Empty;

            if (!IsSpeakerTRUE)
            {
                int selectedParagraphNum;
                int selectedWordNum;


                if (protocolName.Text != null
                && string.IsNullOrWhiteSpace(protocolName.Text) == false
                && paragraphNum.Text != null
                && wordNum.Text != null)
                {
                    selectedProcotocolName = protocolName.Text;
                    selectedParagraphNum = int.Parse(paragraphNum.Text);
                    selectedWordNum = int.Parse(wordNum.Text);


                    if (
                        selectedParagraphNum > 0
                        && selectedWordNum > 0
                        && string.IsNullOrWhiteSpace(selectedProcotocolName) == false
                        )
                    {

                        List<Protocol> relevantProtocols = new List<Protocol>();
                        relevantProtocols = context.Protocols.Where(i => i.pr_title.Contains(selectedProcotocolName)).ToList();


                        searchedWords = context.ParagraphWords.Where(p => ((p.pg_number == selectedParagraphNum
                        &&
                        p.word_number == selectedWordNum)
                        )).ToList();


                        searchedWords = (from p in searchedWords
                                         join i in relevantProtocols
                                         on p.pr_number equals i.pr_number
                                         //where p.c_name == i.c_name
                                         select new ParagraphWord
                                         {
                                             word = p.word,
                                             c_name = i.pr_title
                                         }
                                     ).ToList();
                    }

                }
            }
            ////*******************************************//
            else
            {

                string speakerName = string.Empty;
                int paragraphSpekerNum;
                int paragraphSpekerOffset;


                if (protocolName.Text != null
                    && string.IsNullOrWhiteSpace(protocolName.Text) == false
                    && string.IsNullOrWhiteSpace(SpkeakerName.Text) == false
                    && PgSpeakerNum.Text != null
                    && PgOffset.Text != null)
                {
                    selectedProcotocolName = protocolName.Text;
                    speakerName = SpkeakerName.Text;
                    paragraphSpekerNum = int.Parse(PgSpeakerNum.Text);
                    paragraphSpekerOffset = int.Parse(PgOffset.Text);




                    List<Protocol> relevantProtocols = new List<Protocol>();
                    relevantProtocols = context.Protocols.Where(i => i.pr_title.Contains(selectedProcotocolName)).ToList();

                    List<DBEntities.Paragraph> speakerResults = new List<DBEntities.Paragraph>();
                    speakerResults = context.Paragraphs.Where(p => ((p.pn_pg_number == paragraphSpekerNum
                    &&
                    p.pn_name == speakerName)
                    )).ToList();

                    speakerResults = (from p in speakerResults
                                      join i in relevantProtocols
                                     on p.pr_number equals i.pr_number
                                      select new DBEntities.Paragraph
                                      {
                                          pn_pg_number = p.pn_pg_number,
                                          c_name = i.pr_title,
                                          pn_name = p.pn_name,
                                          pr_number = p.pr_number
                                      }
                                 ).ToList();


                    List<ParagraphWord> speakerWords = new List<ParagraphWord>();
                    speakerWords = context.ParagraphWords.Where(j => j.pg_offset == paragraphSpekerOffset).ToList();

                    searchedWords = (from j in speakerWords
                                     join i in relevantProtocols
                                     on j.pr_number equals i.pr_number
                                     //  where j.pg_offset == paragraphSpekerOffset
                                     select new ParagraphWord
                                     {
                                         word = j.word,
                                         c_name = i.pr_title,
                                         pg_offset = j.pg_offset,
                                         pr_number = j.pr_number
                                     }
                             ).ToList();


                    // joint between temp results 
                    searchedWords = (from p in speakerResults
                                     join j in searchedWords
                                     on p.pr_number equals j.pr_number
                                     where p.pg_number == j.pg_number
                                     select new ParagraphWord
                                     {
                                         word = j.word,
                                         c_name = j.c_name,

                                     }
                               ).ToList();


                }
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

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }



        protected void SpecializedSoftware_CheckedChanged(object sender, EventArgs e)
        {

        }


        private void wordNum_TextChanged(object sender, EventArgs e)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(wordNum.Text, "  ^ [0-9]"))
            {
                wordNum.Text = "";
            }
        }


        private void openChosenProtocol(object sender, SelectionChangedEventArgs e)
        {
            if (lstResults.SelectedIndex == -1) return;
            ProtocolDisplayWindow chosenP = new ProtocolDisplayWindow(lstResults.SelectedItem as Protocol);
            chosenP.ShowDialog();
            lstResults.SelectedIndex = -1;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (checkSpeakerBox.IsChecked == false)
            {
                WordNumSearch.Visibility = Visibility.Hidden;
                SpeakerSearch.Visibility = Visibility.Visible;
            }
            else
            {
                SpeakerSearch.Visibility = Visibility.Hidden;
                WordNumSearch.Visibility = Visibility.Visible;
            }

        }

        [ValueConversion(typeof(bool), typeof(Visibility))]
        public sealed class InverseBooleanToVisibilityConverter : IValueConverter
        {
            private BooleanToVisibilityConverter _converter = new BooleanToVisibilityConverter();

            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                
                var result = _converter.Convert(value, targetType, parameter, culture) as Visibility?;
                return result == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                var result = _converter.ConvertBack(value, targetType, parameter, culture) as bool?;
                return result == true ? false : true;
            }
        }

    }
}