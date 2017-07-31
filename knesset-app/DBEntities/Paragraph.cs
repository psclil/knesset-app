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
        // a property to wrap the ReconstractParagraph() method. both to create a cache layer and to allow it to be used in data templates
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
            // reconstruct the original paragraph text by merging all the words in the correct offset and filling the rest of the chars with pg_space_fillers
            int spaceFillerRead = 0;
            // use a stringbuilder because it's faster and uses less memory than string concatenation
            StringBuilder ret = new StringBuilder();
            foreach (ParagraphWord pWord in words.OrderBy(w => w.pg_offset))
            {
                // fill from pg_space_fillers until we reach the correct index for the current word
                int spaceFillerNeeded = pWord.pg_offset - ret.Length;
                if (spaceFillerNeeded > 0)
                {
                    ret.Append(pg_space_fillers.Substring(spaceFillerRead, spaceFillerNeeded));
                    spaceFillerRead += spaceFillerNeeded;
                }
                // add the current word
                ret.Append(pWord.word);
            }
            // if we've added all the words but need to add some more fillerrs
            if (spaceFillerRead < pg_space_fillers.Length)
            {
                ret.Append(pg_space_fillers.Substring(spaceFillerRead));
            }
            return ret.ToString();
        }

    }
}
