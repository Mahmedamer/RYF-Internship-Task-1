namespace NYFInter.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Message
    {
        public int Id { get; set; }

        [StringLength(128)]
        public string AdminId { get; set; }
        [NotMapped]
        public string SentBy { get; set; }
        [Required]
        public string Text { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime SentDate { get; set; }
    }
}
