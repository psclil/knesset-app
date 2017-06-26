using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace knesset_app.DBEntities
{
    [Table("Phrase")]
    public class Phrase
    {
        [Key,MaxLength(310)]
        public string phrase { get; set; }
    }
}
