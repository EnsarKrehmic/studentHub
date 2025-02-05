using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class StudijskiProgram
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Naziv studijskog programa je obavezan.")]
        [MaxLength(100, ErrorMessage = "Naziv ne može biti duži od 100 karaktera.")]
        public string Naziv { get; set; }

        [MaxLength(500, ErrorMessage = "Opis ne može biti duži od 500 karaktera.")]
        public string Opis { get; set; }

        [Required(ErrorMessage = "Trajanje studijskog programa je obavezno.")]
        [Range(1, 6, ErrorMessage = "Trajanje mora biti između 1 i 6 godina.")]
        [DisplayName("Trajanje (godine)")]
        public int TrajanjeUGodinama { get; set; }
        public List<StudentStudijskiProgram> StudentStudijskiProgrami { get; set; } = new();
        public List<AsistentStudijskiProgram> AsistentStudijskiProgrami { get; set; } = new();
        public List<ProfesorStudijskiProgram> ProfesorStudijskiProgrami { get; set; } = new();
        public List<StudentskaSluzbaStudijskiProgram> StudentskaSluzbaStudijskiProgrami { get; set; } = new();
        public List<ObavjestenjeStudijskiProgram> ObavjestenjeStudijskiProgrami { get; set; } = new();
        public StudijskiProgram() { }
    }
}