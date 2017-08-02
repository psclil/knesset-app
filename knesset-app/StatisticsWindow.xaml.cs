using knesset_app.DBEntities;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace knesset_app
{
    /// <summary>
    /// Interaction logic for StatisticsWindow.xaml
    /// </summary>
    public partial class StatisticsWindow : Window
    {
        public StatisticsWindow()
        {
            InitializeComponent();
        }

        private void InitStatistics(object sender, RoutedEventArgs e)
        {
            // note to db admins,
            // there is no reason this should be slow
            // try setting:
            // query_cache_limit=20K
            // query_cache_size=4M
            // query_cache_type=1
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                var res = new GeneralStatisticsResult();
                using (var context = new KnessetContext())
                {
                    int numWordsSpoken = context.ParagraphWords.Count();
                    int numParagraphs = context.Paragraphs.Count();
                    res.NumProtocols = context.Protocols.Count();
                    res.ParagraphsPerProtocol = (float)numParagraphs / res.NumProtocols;
                    /*
                     * calculate speakers by protocol
                     * the key is that each speaker has only one paragraph with pn_ph_number=1
                     SELECT AVG(sq.c) as v FROM (
	                    SELECT COUNT(*) as c
	                    FROM paragraph p
	                    WHERE p.pn_pg_number = 1
	                    GROUP BY p.c_name, p.pr_number
                    ) as sq
                     */
                    res.SpeakersPerProtocol = (float)context.Paragraphs
                                                        .Where(x => x.pn_pg_number == 1)
                                                        .GroupBy(x => new { x.c_name,x.pr_number })
                                                        .Select(r => r.Count())
                                                        .Average();
                    res.ParagraphsPerProtocolSpeaker = res.ParagraphsPerProtocol / res.SpeakersPerProtocol;
                    res.WordsPerProtocol = (float)numWordsSpoken / res.NumProtocols;
                    res.WordsPerParagraph = (float)numWordsSpoken / numParagraphs;
                }
                DataContext = res;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Mouse.OverrideCursor = null;
                Close();
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void WordFrequencies(object sender, RoutedEventArgs e)
        {
            WordFrequenciesWindow freqWindow = new WordFrequenciesWindow();
            freqWindow.ShowDialog();
        }

        private void Presence(object sender, RoutedEventArgs e)
        {
            PresenceStatisticsWindow presenceWindow = new PresenceStatisticsWindow();
            presenceWindow.ShowDialog();
        }
    }
}
