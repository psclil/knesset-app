using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace knesset_app.DBEntities
{
    [Table("Paragraph")]
    public class Paragraph
    {
        [Key, Column(Order = 1), MaxLength(45)]
        public string c_name { get; set; }

        [Key, Column(Order = 2)]
        public int pr_number { get; set; }

        [Key, Column(Order = 3)]
        public int pg_number { get; set; }

        public string pg_space_fillers { get; set; }

        public int pg_num_words { get; set; }

        [MaxLength(50)]
        public string pn_name { get; set; }

        public int pn_pg_number { get; set; }

        [ForeignKey("c_name,pr_number")]
        public virtual Protocol protocol { get; set; }

        [ForeignKey("pn_name")]
        public Person speaker { get; set; }

        [InverseProperty("paragraph")]
        public virtual ICollection<ParagraphWord> words { get; set; }

        private string _originalText;
        public string OriginalText
        {
            get
            {
                if (_originalText == null) _originalText = ReconstractParagraph();
                return _originalText;
            }
        }

        public string ReconstractParagraph()
        {
            int spaceFillerRead = 0;
            StringBuilder ret = new StringBuilder();
            foreach (ParagraphWord pWord in words.OrderBy(w => w.pg_offset))
            {
                int spaceFillerNeeded = pWord.pg_offset - ret.Length;
                if (spaceFillerNeeded > 0)
                {
                    ret.Append(pg_space_fillers.Substring(spaceFillerRead, spaceFillerNeeded));
                    spaceFillerRead += spaceFillerNeeded;
                }
                ret.Append(pWord.word);
            }
            if (spaceFillerRead < pg_space_fillers.Length)
            {
                ret.Append(pg_space_fillers.Substring(spaceFillerRead));
            }
            return ret.ToString();
        }

    }
}
