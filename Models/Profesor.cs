using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace StudentHub.Models
{
    public class Profesor : Korisnik
    {
        [Required]
        [MaxLength(100)]
        public string Titula { get; set; }

        public Profesor() { }
    }
}

