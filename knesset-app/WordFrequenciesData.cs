using knesset_app.DBEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace knesset_app
{
    class WordFrequenciesData : INotifyPropertyChanged
    {
        public WordFrequenciesData()
        {
            using (KnessetContext context = new KnessetContext())
            {
                var emptyStringList = new string[] { string.Empty };
                AllCommittees = emptyStringList.Union(context.Committees.Select(x => x.c_name).OrderBy(x => x)).ToList();
                AllSpeakers = emptyStringList.Union(context.Paragraphs.Select(x => x.pn_name).Distinct().OrderBy(x => x)).ToList();
            }
            UpdateResults();
        }

        private string _c_name;
        public string c_name
        {
            get => _c_name;
            set
            {
                _c_name = value;
                OnPropertyChanged("c_name");
                UpdateResults();
            }
        }

        private string _pn_name;
        public string pn_name
        {
            get => _pn_name;
            set
            {
                _pn_name = value;
                OnPropertyChanged("pn_name");
                UpdateResults();
            }
        }

        private List<string> allCommittees;
        public List<string> AllCommittees
        {
            get => allCommittees;
            set
            {
                allCommittees = value;
                OnPropertyChanged("AllCommittees");
            }
        }

        private List<string> allSpeakers;
        public List<string> AllSpeakers
        {
            get => allSpeakers; set
            {
                allSpeakers = value;
                OnPropertyChanged("AllSpeakers");
            }
        }

        private BindingList<WordFrequency> results;
        public BindingList<WordFrequency> Results
        {
            get => results; set
            {
                results = value;
                OnPropertyChanged("Results");
            }
        }

        private void UpdateResults()
        {
            using (KnessetContext context = new KnessetContext())
            {
                IQueryable<ParagraphWord> data = context.ParagraphWords;
                if (!string.IsNullOrEmpty(_c_name))
                    data = data.Where(x => x.c_name == _c_name);
                if (!string.IsNullOrEmpty(_pn_name))
                    data = data.Where(x => x.paragraph.pn_name == _pn_name);
                int total = data.Count();
                Results = new BindingList<WordFrequency>(data
                    .GroupBy(x => x.word)
                    .Select(x => new WordFrequency { Word = x.Key, Absolute = x.Count(), Frequency = (float)x.Count() / total })
                    .ToList()
                    .OrderByDescending(x => x.Absolute)
                    .Take(10000)
                    .ToList());
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
