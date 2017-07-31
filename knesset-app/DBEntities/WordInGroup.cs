using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace knesset_app.DBEntities
{
    [Table("WordInGroup")]
    public class WordInGroup
    {
        [Key, Column(Order = 1)]
        public string g_name { get; set; }

        [Key, Column(Order = 2)]
        public string word { get; set; }

        [ForeignKey("g_name")]
        public virtual WordsGroup wordsGroup { get; set; }

        [ForeignKey("word")]
        public virtual Word WordObj { get; set; }
    }
}
