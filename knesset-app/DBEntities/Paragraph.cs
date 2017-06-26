using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public Protocol protocol { get; set; }

        [ForeignKey("pn_name")]
        public Person speaker { get; set; }

        [ForeignKey("paragraph)")]
        public ICollection<ParagraphWord> words { get; set; }
    }
}
