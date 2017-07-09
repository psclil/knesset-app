using knesset_app.DBEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace knesset_app
{
    class CreateIndexData : INotifyPropertyChanged
    {

        public CreateIndexData()
        {
            UpdateData(true, true);
        }

        private List<WordsGroup> allGroup = new List<WordsGroup>();

        public List<WordsGroup> AllGroups
        {
            get => allGroup;
            set
            {
                allGroup = value;
                OnPropertyChanged("AllGroups");
            }
        }

        private List<Protocol> allProtocols = new List<Protocol>();

        public List<Protocol> AllProtocols
        {
            get => allProtocols;
            set
            {
                allProtocols = value;
                OnPropertyChanged("AllProtocols");
            }
        }

        public void UpdateData(bool updateProtocols, bool updateGroups)
        {
            using (KnessetContext context = new KnessetContext())
            {
                if (updateGroups)
                    AllGroups = context.WordsGroups.Include("items").OrderBy(x => x.g_name).ToList();
                if (updateProtocols)
                {
                    var q = from protocol in context.Protocols
                            orderby protocol.c_name, protocol.pr_number
                            select protocol;
                    AllProtocols = q.ToList();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
