using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class Ispit
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Datum održavanja je obavezan.")]
        [DisplayName("Datum održavanja")]
        public DateTime DatumOdrzavanja { get; set; }

        [Required]
        [DisplayName("Datum objave")]
        public DateTime DatumObjave { get; set; } = DateTime.Now;

        [MaxLength(200, ErrorMessage = "Lokacija ne smije biti duža od 200 karaktera.")]
        public string? Lokacija { get; set; }

        [Required(ErrorMessage = "Broj bodova je obavezan.")]
        [Range(0, 100, ErrorMessage = "Ukupni broj bodova mora biti između 0 i 100.")]
        [Display(Name = "Ukupni bodovi ispita")]
        public decimal BrojBodova { get; set; }

        [Range(0, 100, ErrorMessage = "Uslov za polaganje mora biti između 0 i 100.")]
        [Display(Name = "Uslov za polaganje (minimalni broj bodova)")]
        public decimal UslovZaPolaganje { get; set; }

        [Required(ErrorMessage = "Studijski program je obavezan.")]
        [DisplayName("Studijski program")]
        [ForeignKey("StudijskiProgram")]
        public long StudijskiProgramId { get; set; }
        public StudijskiProgram StudijskiProgram { get; set; }

        [Required(ErrorMessage = "Nastavni plan je obavezan.")]
        [DisplayName("Nastavni plan")]
        [ForeignKey("NastavniPlan")]
        public long NastavniPlanId { get; set; }
        public NastavniPlan NastavniPlan { get; set; }

        [Required(ErrorMessage = "Predmet je obavezan.")]
        [DisplayName("Predmet")]
        [ForeignKey("Predmet")]
        public long PredmetId { get; set; }
        public Predmet Predmet { get; set; }

        public Ispit() { }
    }
}