namespace knesset_app
{
    public class GeneralStatisticsResult
    {
        // an object to hold general statistics for this app (used by StatisticsWindow)
        public int NumProtocols { get; set; }
        public float SpeakersPerProtocol { get; set; }
        public float ParagraphsPerProtocol { get; set; }
        public float ParagraphsPerProtocolSpeaker { get; set; }
        public float WordsPerProtocol { get; set; }
        public float WordsPerParagraph { get; set; }
    }
}
