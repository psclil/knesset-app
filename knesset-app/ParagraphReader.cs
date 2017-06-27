using System;
using System.Text;
using System.Text.RegularExpressions;

namespace knesset_app
{
    public class ParagraphReader
    {
        public Regex WordStartChars { get; set; }
        public Regex WordContinueChars { get; set; }

        public ParagraphReader()
        {
            // todo: add more class settings in the future...
            WordStartChars = new Regex("[א-ת0-9a-zA-Z]", RegexOptions.Compiled);
            WordContinueChars = new Regex("[א-ת0-9a-zA-Z\\\"]", RegexOptions.Compiled);
        }

        private enum ReadState
        {
            NotInWord,
            Word
        }

        public void Read(string paragraphContent, Action<string> wordHandler, Action<string> fillerHandler)
        {
            ReadState state = ReadState.NotInWord;
            StringBuilder buffer = new StringBuilder();
            for (int i = 0; i < paragraphContent.Length; i++)
            {
                string curr = paragraphContent.Substring(i, 1);
                switch (state)
                {
                    case ReadState.NotInWord:
                        if (WordStartChars.IsMatch(curr))
                        {
                            if (buffer.Length > 0)
                            {
                                fillerHandler(buffer.ToString());
                                buffer.Clear();
                            }
                            state = ReadState.Word;
                        }
                        break;
                    case ReadState.Word:
                        if (!WordContinueChars.IsMatch(curr))
                        {
                            if (buffer.Length > 0)
                            {
                                wordHandler(buffer.ToString());
                                buffer.Clear();
                            }
                            state = ReadState.NotInWord;
                        }
                        break;
                }
                buffer.Append(curr);
            }
        }
    }
}