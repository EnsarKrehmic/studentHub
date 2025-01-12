using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class Predmet
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Naziv predmeta je obavezan.")]
        [MaxLength(100, ErrorMessage = "Naziv ne može biti duži od 100 karaktera.")]
        public string Naziv { get; set; }

        [MaxLength(500, ErrorMessage = "Opis ne može biti duži od 500 karaktera.")]
        public string Opis { get; set; }

        [Required(ErrorMessage = "ECTS bodovi su obavezni.")]
        [Range(1, 30, ErrorMessage = "Broj ECTS bodova mora biti između 1 i 30.")]
        public int ECTS { get; set; }

        [ForeignKey("Profesor")]
        public long? ProfesorId { get; set; }
        public Profesor Profesor { get; set; }

        [ForeignKey("Asistent")]
        public long? AsistentId { get; set; }
        public Asistent Asistent { get; set; }

        [Required(ErrorMessage = "Nastavni plan je obavezan.")]
        [ForeignKey("NastavniPlan")]
        public long? NastavniPlanId { get; set; }
        public NastavniPlan NastavniPlan { get; set; }

        public Predmet() { }
    }
}