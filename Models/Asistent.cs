using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace StudentHub.Models
{
    public class Asistent : Korisnik
    {
        [Required]
        [MaxLength(100)]
        public string Titula { get; set; }

        public Asistent() { }
    }
}
