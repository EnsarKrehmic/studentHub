using System.ComponentModel.DataAnnotations;

namespace StudentHub.Models
{
    public class Profesor : Korisnik
    {
        [Required(ErrorMessage = "Titula je obavezna.")]
        [MaxLength(50, ErrorMessage = "Titula ne može biti duža od 50 karaktera.")]
        public string Titula { get; set; }

        public Profesor() { }
    }
}

