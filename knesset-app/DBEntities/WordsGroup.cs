using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace knesset_app.DBEntities
{
    [Table("WordsGroup")]
    public class WordsGroup
    {
        public const int MaxItemsInGroup = 20;

        [Key, MaxLength(70)]
        public string g_name { get; set; }

        [InverseProperty("wordsGroup")]
        public virtual ICollection<WordInGroup> items { get; set; }
    }
}
