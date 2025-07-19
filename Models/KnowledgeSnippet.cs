using System;
using System.ComponentModel.DataAnnotations;

namespace StudentHub.Models
{
    public class KnowledgeSnippet
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Izvor { get; set; }

        [Required]
        [StringLength(300)]
        public string Naslov { get; set; }

        [Required]
        [StringLength(3000)]
        public string Sadrzaj { get; set; }

        public DateTime DatumDodavanja { get; set; } = DateTime.Now;
    }
}
