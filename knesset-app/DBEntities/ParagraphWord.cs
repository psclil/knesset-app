using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace knesset_app.DBEntities
{
    [Table("ParagraphWord")]
    public class ParagraphWord
    {
        [Key,Column(Order = 1)]
        public string c_name { get; set; }

        [Key,Column(Order = 2)]
        public int pr_number { get; set; }

        [Key,Column(Order = 3)]
        public int pg_number { get; set; }

        [Key,Column(Order = 4)]
        public int word_number { get; set; }

        public int pg_offset { get; set; }

        public string word { get; set; }

        [ForeignKey("word")]
        public virtual Word WordObj { get; set; }

        [ForeignKey("c_name,pr_number,pg_number")]
        public virtual Paragraph paragraph { get; set; }
    }
}
