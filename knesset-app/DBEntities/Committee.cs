using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace knesset_app.DBEntities
{
    [Table("Committee")]
    public class Committee
    {
        [Key]
        [MaxLength(45)]
        public string c_name { get; set; }

        [InverseProperty("committee")]
        public ICollection<Protocol> protocols { get; set; }
    }
}
