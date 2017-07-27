using knesset_app.DBEntities;
using System.Windows;

namespace knesset_app
{
    /// <summary>
    /// Interaction logic for OpenProtocol.xaml
    /// </summary>
    public partial class ProtocolDisplayWindow : Window
    {
        private Protocol protocol;

        public ProtocolDisplayWindow(Protocol protocol)
        {
            this.protocol = protocol;
            InitializeComponent();
        }
    }
}
