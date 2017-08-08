using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using knesset_app.DBEntities;
using System.Data.Entity;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace knesset_app
{

    /// <summary>
    /// Interaction logic for SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : Window
    {
        int MAX_DISPLAY_RECORD = 1000;
        KnessetContext context = new KnessetContext();


        public SearchWindow()
        {
            Mouse.OverrideCursor = Cursors.Wait; //set mouse cursor to "busy" status while intialize window
            try
            {
                InitializeComponent();
                PopulateCommitteeCombobox();
                PopulatePhraseCombobox();
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }



        private void PopulateCommitteeCombobox()
        {
            cbProtocolCommitte.ItemsSource = context.Committees.Take(MAX_DISPLAY_RECORD).ToList();
            cbProtocolCommitte.DisplayMemberPath = "c_name";
            cbProtocolCommitte.SelectedValuePath = "c_name";

        }

        private void PopulatePhraseCombobox()
        {
            cbPhraseList.ItemsSource = context.Phrases.Take(MAX_DISPLAY_RECORD).ToList();
            cbPhraseList.DisplayMemberPath = "phrase";
            cbPhraseList.SelectedValuePath = "phrase";
        }




        // Middel Tab - Search protocols by dates, protocol title, committee, invited and presence names
        private void MetaDataSearch(object sender, RoutedEventArgs e)
        {
            //reset\hide "No results" message
            noResultsMessageMetaData.Visibility = Visibility.Hidden;
            IQueryable<Protocol> relevantProtocols = context.Protocols;

            
            Mouse.OverrideCursor = Cursors.Wait; //set mouse cursor to "busy" status while searching & preparing results
            try
            {
            string protocolTitle = string.IsNullOrEmpty(tbProtocolTitle.Text) ? string.Empty : tbProtocolTitle.Text;
            string selectedProcotocolCommitte = string.Empty;

            //getting the date fields 
            DateTime? fromDate = dpFromDate.SelectedDate;
            DateTime? toDate = dpToDate.SelectedDate;

           
                //commettie field 
                if (cbProtocolCommitte.Text != null && string.IsNullOrWhiteSpace(cbProtocolCommitte.Text) == false)
                {
                    selectedProcotocolCommitte = cbProtocolCommitte.Text;
                }

                if (!string.IsNullOrEmpty(selectedProcotocolCommitte))
                    relevantProtocols = relevantProtocols.Where(x => x.c_name == selectedProcotocolCommitte);
                if (!string.IsNullOrEmpty(protocolTitle))
                    relevantProtocols = relevantProtocols.Where(x => x.pr_title.Contains(protocolTitle));
                if (fromDate.HasValue)
                    relevantProtocols = relevantProtocols.Where(x => x.pr_date >= fromDate);
                if (toDate.HasValue)
                    relevantProtocols = relevantProtocols.Where(x => x.pr_date <= toDate);



                //Invited field - performing an AND relation between given invited list
                char[] nameListSplit = new char[] { ',' };

                if (!string.IsNullOrWhiteSpace(tbInvited.Text))
                {
                    string[] split = tbInvited.Text.Split(nameListSplit, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < split.Length; i++)
                    {
                        split[i] = split[i].Trim();
                    }

                    foreach (string temps in split)
                    {
                        relevantProtocols = relevantProtocols.Where(x => x.invitations.Any(i => i.pn_name.Contains(temps)));
                    }
                }



                //Presences field - performing an AND relation between given presences list
                if (string.IsNullOrWhiteSpace(tbPersence.Text) == false)
                {
                    string[] split = tbPersence.Text.Split(nameListSplit, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < split.Length; i++)
                    {
                        split[i] = split[i].Trim();
                    }

                    foreach (string temps in split)
                    {
                        relevantProtocols = relevantProtocols.Where(x => x.persence.Any(i => i.pn_name.Contains(temps)));
                    }
                }




                // Display results
                List<Protocol> protocols = relevantProtocols.ToList();

                if (protocols != null && protocols.Count > 0)
                {
                    lstResults.ItemsSource = protocols;
                }
                else
                {
                    // Display "NO results" message
                    lstResults.ItemsSource = string.Empty;
                    noResultsMessageMetaData.Visibility = Visibility.Visible;
                }
            }
            finally //set mouse cursor back to normal
            {
                Mouse.OverrideCursor = null;
            }
        }





        // Right Tab - Search a word in protocols by: word's location parameters OR by speaker's parameters
        private void BackwardsSearch(object sender, RoutedEventArgs e)
        {
            //reset\hide "No results" message
            noResultsMessageBackward.Visibility = Visibility.Hidden;

            IQueryable<ParagraphWord> searchedWords;
            string selectedProcotocolName = string.Empty;

            Mouse.OverrideCursor = Cursors.Wait; //set mouse cursor to "busy" status while searching & preparing results
            try
            {
                //Search with word's location parameters
                if (!speakerCheckBox.IsChecked.GetValueOrDefault())
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
                                 );
                }

                //Search with Speaker parameters
                else
                {
                    if (string.IsNullOrWhiteSpace(protocolName.Text) || string.IsNullOrWhiteSpace(speakerName.Text)
                        || string.IsNullOrWhiteSpace(pgSpeakerNum.Text) || string.IsNullOrWhiteSpace(pgOffset.Text))
                    {
                        MessageBox.Show("חובה למלא את כל השדות");
                        return;
                    }
                    string selectedSpeakerName = string.Empty;
                    selectedProcotocolName = protocolName.Text;
                    selectedSpeakerName = speakerName.Text;
                    int paragraphSpekerNum = int.Parse(pgSpeakerNum.Text);
                    int paragraphSpekerOffset = int.Parse(pgOffset.Text);

                    // result is a product of 3 different tables - therefore 2 join operations were executed
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
                                     r.pn_name.Contains(selectedSpeakerName)
                                     &&
                                     r.pn_pg_number == paragraphSpekerNum
                                     select p
                                 );
                }

                // preparing results and limit the number of results to MAX_DISPLAY_RECORD
                var results = searchedWords.Take(MAX_DISPLAY_RECORD).Include("paragraph.words").ToList().Select(w => new ParagraphMatch(w.paragraph, w, new List<string> { w.word })).ToList();

                // Display results
                if (results.Count > 0)
                {
                    lstBackwardSearchResults.ItemsSource = results;
                }
                else
                {
                    // Display "NO results" message\
                    lstBackwardSearchResults.ItemsSource = string.Empty;
                    noResultsMessageBackward.Visibility = Visibility.Visible;
                }

            }
            finally //set mouse cursor back to normal
            {
                Mouse.OverrideCursor = null;
            }

        }


        // Left Tab - search protocols according to word\s
        private void PhraseSearch(object sender, RoutedEventArgs e)
        {
            //reset\hide "No results" message
            noResultsMessagePhrase.Visibility = Visibility.Hidden;

            Mouse.OverrideCursor = Cursors.Wait; //set mouse cursor to "busy" status while searching & preparing results
            try
            {
                if (string.IsNullOrWhiteSpace(cbPhraseList.Text))
            {
                MessageBox.Show("חובה להכניס ביטוי לחיפוש");
                return;
            }
            char[] nameListSplit = new char[] { ' ' };

            string selectedExpression = cbPhraseList.Text;
            ParagraphReader paragraphReader = new ParagraphReader();
            List<string> searchWords = paragraphReader.ReadWords(selectedExpression);

            //limits the numbers of words in the phrase search
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

                // fetch results
                var resultsRaw = selecetedExpression.Take(MAX_DISPLAY_RECORD).Include("paragraph.words").ToList();
                // highlight search phrase and create a result snippet for each result
                var results = resultsRaw.Select(x => new ParagraphMatch(x.paragraph, x, searchWords)).ToList();

                if (results.Count > 0)
                {
                    // display results
                    lstPhraseSearchResults.ItemsSource = results;
                }
                else
                {
                    // Display "NO results" message
                    lstPhraseSearchResults.ItemsSource = string.Empty;
                    noResultsMessagePhrase.Visibility = Visibility.Visible;
                }

            }
            finally //set mouse cursor back to normal
            {
                Mouse.OverrideCursor = null;
            }



        }


        // Add  given Phrase input to "Phrase" table in DB
        private void AddPhrase(object sender, RoutedEventArgs e)
        {
            string phraseToAdd = string.IsNullOrEmpty(cbPhraseList.Text) ? string.Empty : cbPhraseList.Text.Trim();
            if (string.IsNullOrWhiteSpace(phraseToAdd))
            {
                MessageBox.Show("ערך לא חוקי");
                cbPhraseList.Text = string.Empty;
                return;
            }
            try
            {
                Phrase existing = context.Phrases.Find(phraseToAdd);
                if (existing != null)
                {
                    MessageBox.Show("כבר קיים ביטוי כזה");
                    return;
                }
                Phrase phrase = new Phrase { phrase = phraseToAdd };
                context.Phrases.Add(phrase);
                context.SaveChanges();
                PopulatePhraseCombobox();
                cbPhraseList.SelectedItem = phrase; // update selection to the new object
                PhraseSelectionChanged(); // update add/delete buttons
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        // Delete given Phrase input from "Phrase" table in DB
        private void DeletePhrase(object sender, RoutedEventArgs e)
        {
            if (cbPhraseList.SelectedIndex == -1) return;
            Phrase toRemove = (Phrase)cbPhraseList.SelectedItem;
            context.Phrases.Remove(toRemove);
            context.SaveChanges();
            cbPhraseList.SelectedIndex = -1;
            PopulatePhraseCombobox();
            PhraseSelectionChanged();

        }

        #region ui-only-code

        private void PhraseSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PhraseSelectionChanged();
        }


        private void PhraseKeyUp(object sender, KeyEventArgs e)
        {
            PhraseSelectionChanged();
        }

        // handels the enabling of "add\remove" buttons in Phrase search tab
        private void PhraseSelectionChanged()
        {
            object selectedItem = cbPhraseList.SelectedItem;
            if (selectedItem == null)
            {
                btnAddPhrase.IsEnabled = cbPhraseList.Text.Length > 0; //checks if there's an input in text field
                btnRemovePhrase.IsEnabled = false;
            }
            else
            {
                btnAddPhrase.IsEnabled = false;
                btnRemovePhrase.IsEnabled = true;
            }
        }


        //Open selected protocol from MetaData search listbox
        private void OpenChosenProtocol(object sender, SelectionChangedEventArgs e)
        {

            if (lstResults.SelectedIndex == -1) return;
            ProtocolDisplayWindow chosenP = new ProtocolDisplayWindow(lstResults.SelectedItem as Protocol);
            chosenP.ShowDialog();
            lstResults.SelectedIndex = -1;
        }

        //Open selected protocol from backwards or Phrase search listbox
        private void OpenChosenPhraseProtocol(object sender, SelectionChangedEventArgs e)
        {
            if (sender == null || !(sender is ListBox)) return;
            ListBox lst = sender as ListBox;
            if (lst.SelectedIndex == -1) return;
            ParagraphMatch selectedResultItem = (ParagraphMatch)lst.SelectedItem;
            ProtocolDisplayWindow chosenP = new ProtocolDisplayWindow(selectedResultItem.InParagraph.protocol, selectedResultItem.InParagraph);
            chosenP.ShowDialog();
            lst.SelectedIndex = -1;
        }



        private void ClearAllSearchFields(object sender, RoutedEventArgs e)
        {
            // MetaData Tab
            tbProtocolTitle.Text = string.Empty;
            cbProtocolCommitte.Text = string.Empty;
            tbInvited.Text = null;
            tbPersence.Text = null;
            dpFromDate.SelectedDate = dpToDate.SelectedDate = null;
            lstResults.ItemsSource = string.Empty;
            noResultsMessageMetaData.Visibility = Visibility.Hidden;


            // Backward Tab
            protocolName.Text = string.Empty;
            pgOffset.Text = string.Empty;
            speakerName.Text = string.Empty;
            pgSpeakerNum.Text = null;
            paragraphNum.Text = null;
            wordNum.Text = null;
            lstBackwardSearchResults.ItemsSource = string.Empty;
            noResultsMessageBackward.Visibility = Visibility.Hidden;


            // Phrase Tab
            cbPhraseList.Text = string.Empty;
            cbPhraseList.SelectedIndex = -1;
            lstPhraseSearchResults.ItemsSource = string.Empty;
            noResultsMessagePhrase.Visibility = Visibility.Hidden;
        }



        // prevents adding non numeric input in numeric only fields
        private void NumericTxtChange(object sender, EventArgs e)
        {
            if (sender == null || !(sender is TextBox)) return;
            TextBox textBox = sender as TextBox;
            if (!Regex.IsMatch(textBox.Text, "^[0-9]*$"))
            {
                textBox.Text = Regex.Replace(textBox.Text, "[^0-9]", "");
            }
        }




        // show & hide speaker's search fields in Backwards tab according to checkbox status
        private void CheckBoxStatus(object sender, RoutedEventArgs e)
        {
            if (speakerCheckBox.IsChecked == false)
            {
                speakerSearch.Visibility = Visibility.Hidden;
                wordNumSearch.Visibility = Visibility.Visible;
            }
            else
            {
                wordNumSearch.Visibility = Visibility.Hidden;
                speakerSearch.Visibility = Visibility.Visible;
            }

        }

        #endregion

    }
}