using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace StudentHub.Models
{
    public class Asistent : Korisnik
    {
        [MaxLength(30, ErrorMessage = "Titula ne može biti duža od 30 karaktera.")]
        [DisplayName("Titula")]
        public string? AsistentTitula { get; set; }
        public Asistent() { }
    }
}
