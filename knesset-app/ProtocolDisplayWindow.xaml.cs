using knesset_app.DBEntities;
using System.Windows;
using System.Data.Entity;
using System;
using System.Linq;

namespace knesset_app
{
    public partial class ProtocolDisplayWindow : Window
    {
        private Protocol protocol;

        public ProtocolDisplayWindow(Protocol protocol)
        {
            InitializeComponent();
            LoadProtocol(protocol);
            DataContext = this.protocol;
        }

        private void LoadProtocol(Protocol p)
        {
            using (KnessetContext context = new KnessetContext())
            {
                // load protocol + paragraphs + words
                protocol = context.Protocols.Include("paragraphs.words").First(x => x.c_name == p.c_name && x.pr_number == p.pr_number);
                var entry = context.Entry(protocol);
                entry.Collection(pr => pr.persence).Load(); // load presence
                entry.Collection(pr => pr.invitations).Load(); // load invitations
            }
        }
    }
}
