using StudentHub.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using StudentHub.Data;

namespace StudentHub.Models
{
    public class Student : Korisnik
    {
        [MaxLength(20, ErrorMessage = "Broj indeksa ne može biti duži od 20 karaktera.")]
        [Display(Name = "Broj indeksa")]
        public string? BrojIndeksa { get; set; }

        [MaxLength(200, ErrorMessage = "Prethodno obrazovanje ne može biti duže od 200 karaktera.")]
        [Display(Name = "Predhodno obrazovanje")]
        public string? PredhodnoObrazovanje { get; set; }

        [Range(1, 6, ErrorMessage = "Godina studija mora biti između 1 i 6.")]
        [Display(Name = "Godina studija")]
        public int? GodinaStudija { get; set; }

        [Range(1, 12, ErrorMessage = "Semestar mora biti između 1 i 12.")]
        public int? Semestar { get; set; }

        [ForeignKey("StudijskiProgram")]
        [Display(Name = "Studijski program")]
        public long StudijskiProgramId { get; set; }
        public StudijskiProgram StudijskiProgram { get; set; }

        [ForeignKey("NastavniPlan")]
        [Display(Name = "Nastavni plan")]
        public long? NastavniPlanId { get; set; }
        public NastavniPlan NastavniPlan { get; set; }

        [ForeignKey("Predmet")]
        [Display(Name = "Predmet/i")]
        public long? PredmetId { get; set; }
        public Predmet Predmet { get; set; }

        public bool IsEnrolledInPredmet(long predmetId, ApplicationDbContext context)
        {
            return context.StudentiNaPredmetima.Any(snp => snp.StudentId == this.Id && snp.PredmetId == predmetId);
        }

        public Student() { }
    }
}