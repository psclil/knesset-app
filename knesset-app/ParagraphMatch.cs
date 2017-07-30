using knesset_app.DBEntities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;

namespace knesset_app
{
    public class ParagraphMatch
    {
        const int MAX_WORDS_PADDING = 15;

        public Inline Content { get; protected set; }
        public DBEntities.Paragraph InParagraph { get; protected set; }

        public ParagraphMatch(DBEntities.Paragraph paragraph, ParagraphWord firstWordInMatch, List<string> searchWords)
        {
            InParagraph = paragraph;
            Span content = new Span();
            int firstIncludeWord = firstWordInMatch.word_number - MAX_WORDS_PADDING,
                lastWordInMatch = firstWordInMatch.word_number + searchWords.Count + MAX_WORDS_PADDING,
                spaceFillerRead = 0, charsRead = 0;
            ReadState state = ReadState.Ignore;
            StringBuilder buffer = new StringBuilder();
            foreach (ParagraphWord pWord in paragraph.words.OrderBy(w => w.pg_offset))
            {
                if (state == ReadState.Ignore && pWord.word_number >= firstIncludeWord && pWord.word_number <= lastWordInMatch)
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
                            content.Inlines.Add(new Run(buffer.ToString()) { Background = new SolidColorBrush { Color = Colors.Yellow } });
                            buffer.Clear();
                            state = ReadState.AfterHighlight;
                        }
                        charsRead += pWord.word.Length;
                        break;
                    case ReadState.AfterHighlight:
                        if (pWord.word_number > lastWordInMatch)
                            state = ReadState.Ignore;
                        else
                        {
                            buffer.Append(pWord.word);
                            charsRead += pWord.word.Length;
                        }
                        break;
                    default:
                        charsRead += pWord.word.Length;
                        break;
                }
            }
            if (buffer.Length > 0)
            {
                content.Inlines.Add(new Run(buffer.ToString()));
            }
            Content = content;
        }

        enum ReadState
        {
            Ignore, BeforeHighlight, Highlight, AfterHighlight
        }
    }
}
