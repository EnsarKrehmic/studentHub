using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class Asistent : Korisnik
    {
        [MaxLength(30, ErrorMessage = "Titula ne može biti duža od 30 karaktera.")]
        [DisplayName("Titula")]
        public string? AsistentTitula { get; set; }
        public List<PredmetAsistent> PredmetAsistenti { get; set; } = new List<PredmetAsistent>();
        public List<AsistentStudijskiProgram> AsistentStudijskiProgrami { get; set; }

        public Asistent() { }
    }
}
