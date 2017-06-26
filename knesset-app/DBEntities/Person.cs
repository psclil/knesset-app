﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace knesset_app.DBEntities
{
    [Table("Person")]
    public class Person
    {
        [Key, MaxLength(50)]
        public string pn_name { get; set; }

        [InverseProperty("person")]
        public ICollection<Invitation> invitations { get; set; }

        [InverseProperty("person")]
        public ICollection<Presence> presence { get; set; }

        [InverseProperty("speaker")]
        public ICollection<Paragraph> paragraphs { get; set; }
    }
}
