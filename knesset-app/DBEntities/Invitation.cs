using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace knesset_app.DBEntities
{
    [Table("Invitation")]
    public class Invitation
    {
        [Key, Column(Order = 1), MaxLength(45)]
        public string c_name { get; set; }

        [Key, Column(Order = 2)]
        public int pr_number { get; set; }

        [Key, Column(Order = 3), MaxLength(50)]
        public string pn_name { get; set; }

        [ForeignKey("pn_name")]
        public Person person { get; set; }

        [ForeignKey("c_name,pr_number")]
        public Protocol protocol { get; set; }
    }
}
