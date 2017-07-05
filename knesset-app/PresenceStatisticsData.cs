using knesset_app.DBEntities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System;

namespace knesset_app
{
    class PresenceStatisticsData : INotifyPropertyChanged
    {
        private List<string> allCommittees;
        public List<string> AllCommittees
        {
            get => allCommittees; set
            {
                allCommittees = value;
                OnPropertyChanged("AllCommittees");
            }
        }

        private string _c_name;
        public string c_name
        {
            get => _c_name; set
            {
                _c_name = value;
                OnPropertyChanged("c_name");
                updateResults();
            }
        }

        private void updateResults()
        {
            using (KnessetContext context = new KnessetContext())
            {
                int numProtocols = context.Protocols.Where(p => p.c_name == _c_name).Count();
                if (numProtocols == 0)
                    Results = new BindingList<PresenceStatisticsItem>();
                else
                    Results = new BindingList<PresenceStatisticsItem>(
                        context.Persence
                        .Where(p => p.c_name == _c_name)
                        .GroupBy(p => p.pn_name)
                        .Select(grp => new PresenceStatisticsItem { pn_name = grp.Key, Presence = (float)grp.Count() / numProtocols })
                        .OrderByDescending(x => x.Presence)
                        .ToList()
                    );
            }
        }

        private BindingList<PresenceStatisticsItem> results;
        public BindingList<PresenceStatisticsItem> Results
        {
            get => results;
            set
            {
                results = value;
                OnPropertyChanged("Results");
            }
        }

        public PresenceStatisticsData()
        {
            using (KnessetContext context = new KnessetContext())
            {
                AllCommittees = context.Committees.Select(x => x.c_name).ToList();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
