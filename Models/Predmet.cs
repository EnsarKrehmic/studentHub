using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class Predmet
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Naziv { get; set; }

        [MaxLength(500)]
        public string Opis { get; set; }

        [Required]
        [Range(1, 30)]
        public int ECTS { get; set; }

        [Required]
        [ForeignKey("Profesor")]
        public long ProfesorId { get; set; }
        public Profesor Profesor { get; set; }

        [ForeignKey("Asistent")]
        public long? AsistentId { get; set; }
        public Asistent Asistent { get; set; }

        [Required]
        [ForeignKey("NastavniPlan")]
        public long NastavniPlanId { get; set; }
        public NastavniPlan NastavniPlan { get; set; }

        public Predmet() { }
    }
}