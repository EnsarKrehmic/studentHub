using System.ComponentModel.DataAnnotations;

namespace StudentHub.Models
{
    public class FaqPitanje
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Kategorija { get; set; }
        [Required]
        [StringLength(300)]
        public string Pitanje { get; set; }

        [Required]
        [StringLength(2000)]
        public string Odgovor { get; set; }

        public bool Preporuceno { get; set; }
    }
}
