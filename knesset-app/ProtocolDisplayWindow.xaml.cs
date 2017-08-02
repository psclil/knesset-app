using knesset_app.DBEntities;
using System.Windows;
using System.Linq;

namespace knesset_app
{
    public partial class ProtocolDisplayWindow : Window
    {
        private Protocol protocol;
        private Paragraph initialParagraph;

        public ProtocolDisplayWindow(Protocol protocol, Paragraph initialParagraph = null)
        {
            InitializeComponent();
            LoadProtocol(protocol, initialParagraph);
            DataContext = this.protocol;
        }

        private void LoadProtocol(Protocol p, Paragraph initialPg)
        {
            using (KnessetContext context = new KnessetContext())
            {
                // load protocol + paragraphs + words
                protocol = context.Protocols.Include("paragraphs.words").First(x => x.c_name == p.c_name && x.pr_number == p.pr_number);
                var entry = context.Entry(protocol);
                entry.Collection(pr => pr.persence).Load(); // load presence
                entry.Collection(pr => pr.invitations).Load(); // load invitations
                if (initialPg != null)
                    initialParagraph = protocol.paragraphs.FirstOrDefault(pg => pg.pg_number == initialPg.pg_number);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (initialParagraph != null)
            {
                lstParagraphs.ScrollIntoView(initialParagraph);
                lstParagraphs.SelectedItem = initialParagraph;
            }
            lstParagraphs.Focus();
        }
    }
}
