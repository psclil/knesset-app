using knesset_app.DBEntities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace knesset_app
{
    public class ParagraphMatch
    {
        // a class to construct a search match snippet from a paragraph & search expression
        // into a formatted text object (which Mictosoft calls an Inline)

        const int MAX_WORDS_PADDING = 15;

        public Inline Content { get; protected set; }
        public DBEntities.Paragraph InParagraph { get; protected set; }

        public ParagraphMatch(DBEntities.Paragraph paragraph, ParagraphWord firstWordInMatch, List<string> searchWords)
        {
            // to understand how we reconstruct the paragraph please see a simple version of this code in the method DBEntities.Paragraph.ReconstractParagraph
            InParagraph = paragraph;
            // calculate bounderies, it's ok if it overflows the words list bounderies
            int firstIncludeWord = firstWordInMatch.word_number - MAX_WORDS_PADDING,
                lastWordInMatch = firstWordInMatch.word_number + searchWords.Count + MAX_WORDS_PADDING,
                spaceFillerRead = 0, charsRead = 0;
            // some general ui elements:
            Span content = new Span(new Run(string.Format("{0}: ", paragraph.pn_name)) { FontStyle = FontStyles.Italic }); // add the speaker name
            if (firstIncludeWord >= 0)
                content.Inlines.Add(new Run("... "));  // mark this as a snippet
            ReadState state = ReadState.Ignore; // we ignore content until we reach firstIncludeWord
            StringBuilder buffer = new StringBuilder();
            foreach (ParagraphWord pWord in paragraph.words.OrderBy(w => w.pg_offset))
            {
                if (state == ReadState.Ignore && pWord.word_number >= firstIncludeWord && pWord.word_number <= lastWordInMatch)
                    // reached first word to show
                    state = ReadState.BeforeHighlight;
                int spaceFillerNeeded = pWord.pg_offset - charsRead;
                if (spaceFillerNeeded > 0)
                {
                    if (state != ReadState.Ignore)
                        buffer.Append(paragraph.pg_space_fillers.Substring(spaceFillerRead, spaceFillerNeeded));
                    spaceFillerRead += spaceFillerNeeded;
                    charsRead += spaceFillerNeeded;
                }
                switch (state)
                {
                    case ReadState.BeforeHighlight:
                        if (pWord.word_number == firstWordInMatch.word_number)
                        {
                            // we've reached first word to highlight, add any buffer and jump to next state
                            content.Inlines.Add(new Run(buffer.ToString()));
                            buffer.Clear();
                            state = ReadState.Highlight;
                            goto case ReadState.Highlight;
                        }
                        buffer.Append(pWord.word);
                        charsRead += pWord.word.Length;
                        break;
                    case ReadState.Highlight:
                        buffer.Append(pWord.word);
                        if (pWord.word_number == firstWordInMatch.word_number + searchWords.Count - 1)
                        {
                            // we've just added the last word to highlight
                            content.Inlines.Add(new Run(buffer.ToString()) { Background = new SolidColorBrush { Color = Colors.Yellow } });
                            buffer.Clear();
                            state = ReadState.AfterHighlight;
                        }
                        charsRead += pWord.word.Length;
                        break;
                    case ReadState.AfterHighlight:
                        // add all words until we go out of boundery.
                        if (pWord.word_number > lastWordInMatch)
                            state = ReadState.Ignore;
                        else
                        {
                            buffer.Append(pWord.word);
                            charsRead += pWord.word.Length;
                        }
                        break;
                    default:
                        // even if we ignore words at the begining of the paragraph we still need to count
                        // their length so we know where to push the space fillers
                        charsRead += pWord.word.Length;
                        break;
                }
            }
            if (buffer.Length > 0)
            {
                // add any last space fillers (if the match is close to the paragraph end)
                content.Inlines.Add(new Run(buffer.ToString()));
            }
            if (state == ReadState.Ignore) // snippet is cut at the end...
                content.Inlines.Add(new Run(" ...")); // once more mark this as a snippet
            Content = content;
        }

        enum ReadState
        {
            Ignore, BeforeHighlight, Highlight, AfterHighlight
        }
    }
}
