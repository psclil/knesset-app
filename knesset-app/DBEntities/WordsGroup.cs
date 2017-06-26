using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace knesset_app.DBEntities
{
    [Table("WordsGroup")]
    public class WordsGroup
    {
        [Key, MaxLength(70)]
        public string g_name { get; set; }

        [InverseProperty("wordsGroup")]
        public ICollection<WordInGroup> items { get; set; }
    }
}
