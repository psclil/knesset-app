using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace knesset_app.DBEntities
{
    [Table("Word")]
    public class Word
    {
        [Key,MaxLength(50)]
        public string word { get; set; }

        [InverseProperty("WordObj")]
        public virtual ICollection<WordInGroup> groups { get; set; }

        [InverseProperty("WordObj")]
        public virtual ICollection<ParagraphWord> paragraphs { get; set; }
    }
}
