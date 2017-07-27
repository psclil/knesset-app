using knesset_app.DBEntities;
using System.Windows;
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
                protocol = context.Protocols.Find(p.c_name, p.pr_number); // reload the protocol in the current context
                context.Paragraphs.Include("words").Where(x => x.c_name == protocol.c_name && x.pr_number == protocol.pr_number).ToList(); // preload all the paragraph words
                var tmp = protocol.paragraphs; // load all the paragraphs so we can display them
            }
        }
    }
}
