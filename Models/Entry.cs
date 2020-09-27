namespace NYFInter
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Entry
    {
        public int Id { get; set; }

        [StringLength(128)]
        public string UserId { get; set; }
        [NotMapped]
        public string Username { get; set; }
        [Required]
        public string Text { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "In Query";
    }
}
