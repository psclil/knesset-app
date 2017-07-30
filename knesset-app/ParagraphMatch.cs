
using System.Windows;
using System.Windows.Documents;

namespace knesset_app
{
    class ParagraphMatch
    {
        private Inline Content;
        private Paragraph subParagraphResults;



        public object ReconstractParagraph66698822112121(int wordNum, int wordOffset)
        {

            var SpanResult = new Span();
            SpanResult.Inlines.Add(new Run("text befor"));
            SpanResult.Inlines.Add(new Run("expression") { FontWeight = FontWeights.Bold });
            SpanResult.Inlines.Add(new Run("tex after "));

            return SpanResult;
        }

    }

    
}
