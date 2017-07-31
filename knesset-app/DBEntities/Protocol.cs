using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace knesset_app.DBEntities
{
    [Table("Protocol")]
    public class Protocol
    {
        [Key,MaxLength(45)]
        [Column(Order = 1)]
        public string c_name { get; set; }

        [Key]
        [Column(Order = 2)]
        public int pr_number { get; set; }

        public DateTime pr_date { get; set; }

        [MaxLength(200)]
        public string pr_title { get; set; }

        [ForeignKey("c_name")]
        public virtual Committee committee { get; set; }

        [InverseProperty("protocol")]
        public virtual ICollection<Paragraph> paragraphs { get; set; }

        [InverseProperty("protocol")]
        public virtual ICollection<Presence> persence { get; set; }

        [InverseProperty("protocol")]
        public virtual ICollection<Invitation> invitations { get; set; }
    }
}
