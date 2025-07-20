using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class Profesor : Korisnik
    {
        [MaxLength(30, ErrorMessage = "Titula ne može biti duža od 30 karaktera.")]
        [DisplayName("Titula")]
        public string? ProfesorTitula { get; set; }
        public List<PredmetProfesor> PredmetProfesori { get; set; } = new List<PredmetProfesor>();
        public List<ProfesorStudijskiProgram> ProfesorStudijskiProgrami { get; set; }

        public Profesor() { }
    }
}

