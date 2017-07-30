using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace knesset_app
{
    /**
     * a class to parse protocols in office xml format
     */
    public class ParagraphReader
    {
        public Regex WordStartChars { get; set; } // a regex to find new words start
        public Regex WordContinueChars { get; set; } // a regex to check if still in word or the word has ended

        public ParagraphReader()
        {
            WordStartChars = new Regex("[א-ת0-9a-zA-Z]", RegexOptions.Compiled); // initialize the regular expressions,
            WordContinueChars = new Regex("[א-ת0-9a-zA-Z\\\"]", RegexOptions.Compiled); // though the user might choose to change them.
        }

        /**
         * read a paragraph and run an action provided by the user for each word
         * or another action for the whitespace chars in between words.
         */
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
                            // we're not in a word but a word is about to start.
                            if (buffer.Length > 0)
                            {
                                fillerHandler(buffer.ToString()); // activate the callback for the current filler
                                buffer.Clear();
                            }
                            state = ReadState.Word;
                        }
                        break;
                    case ReadState.Word:
                        if (!WordContinueChars.IsMatch(curr))
                        {
                            // we're in a word but it ended.
                            if (buffer.Length > 0)
                            {
                                wordHandler(buffer.ToString()); // activate the callback for the current word
                                buffer.Clear();
                            }
                            state = ReadState.NotInWord;
                        }
                        break;
                }
                buffer.Append(curr);
            }
            if (buffer.Length > 0)
            {
                // handle the last part of the paragraph
                if (state == ReadState.NotInWord)
                    fillerHandler(buffer.ToString());
                else
                    wordHandler(buffer.ToString());
            }
        }

        public List<string> ReadWords(string paragraphContent)
        {
            List<string> ret = new List<string>();
            Read(paragraphContent, w => ret.Add(w), s => { });
            return ret;
        }

        private enum ReadState
        // just an helper to enable paragraph reading state machine
        {
            NotInWord,
            Word
        }
    }
}