using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace StudentHub.Models
{
    public class Profesor : Korisnik
    {
        [MaxLength(30, ErrorMessage = "Titula ne može biti duža od 30 karaktera.")]
        [DisplayName("Titula")]
        public string? ProfesorTitula { get; set; }
        public Profesor() { }
    }
}

