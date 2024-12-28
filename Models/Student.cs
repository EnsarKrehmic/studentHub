using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class Student : Korisnik
    {
        [Required(ErrorMessage = "Broj indeksa je obavezan.")]
        [MaxLength(20, ErrorMessage = "Broj indeksa ne može biti duži od 20 karaktera.")]
        public string brojIndeksa { get; set; }

        [MaxLength(200, ErrorMessage = "Prethodno obrazovanje ne može biti duže od 200 karaktera.")]
        public string predhodnoObrazovanje { get; set; }

        [Required(ErrorMessage = "Godina studija je obavezna.")]
        [Range(1, 6, ErrorMessage = "Godina studija mora biti između 1 i 6.")]
        public int godinaStudija { get; set; }

        public string podaciUplata { get; set; }

        [Required(ErrorMessage = "Studijski program je obavezan.")]
        [ForeignKey("StudijskiProgram")]
        public long StudijskiProgramId { get; set; }
        public StudijskiProgram studijskiProgram { get; set; }

        public Student() { }
    }
}
