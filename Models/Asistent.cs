using System.ComponentModel.DataAnnotations;

namespace StudentHub.Models
{
    public class Asistent : Korisnik
    {
        [Required(ErrorMessage = "Titula je obavezna.")]
        [MaxLength(50, ErrorMessage = "Titula ne može biti duža od 50 karaktera.")]
        public string Titula { get; set; }

        public Asistent() { }
    }
}
