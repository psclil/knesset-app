using knesset_app.DBEntities;

namespace knesset_app
{
    class WordFrequency
    {
        public WordFrequency()
        {

        }

        public string Word { get; set; }
        public float Frequency { get; set; }
        public int Absolute { get; set; }
    }
}
