using StudentHub.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace StudentHub.Models
{
    public class Student : Korisnik
    {
        [MaxLength(20, ErrorMessage = "Broj indeksa ne može biti duži od 20 karaktera.")]
        [DisplayName("Broj indeksa")]
        public string? BrojIndeksa { get; set; }

        [MaxLength(200, ErrorMessage = "Prethodno obrazovanje ne može biti duže od 200 karaktera.")]
        [DisplayName("Predhodno obrazovanje")]
        public string? PredhodnoObrazovanje { get; set; }

        [Range(1, 6, ErrorMessage = "Godina studija mora biti između 1 i 6.")]
        [DisplayName("Godina studija")]
        public int? GodinaStudija { get; set; }

        [ForeignKey("StudijskiProgram")]
        public long StudijskiProgramId { get; set; }
        public StudijskiProgram StudijskiProgram { get; set; }

        [ForeignKey("NastavniPlan")]
        public long? NastavniPlanId { get; set; }
        public NastavniPlan NastavniPlan { get; set; }

        [Range(1, 12, ErrorMessage = "Semestar mora biti između 1 i 12.")]
        public int? Semestar { get; set; }

        [ForeignKey("Predmet")]
        public long? PredmetId { get; set; }
        public Predmet Predmet { get; set; }

        public Student() { }
    }
}