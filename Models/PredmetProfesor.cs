using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace StudentHub.Models
{
    public class PredmetProfesor
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Predmet je obavezan.")]
        [ForeignKey("Predmet")]
        public long PredmetId { get; set; }
        public Predmet Predmet { get; set; }

        [Required(ErrorMessage = "Profesor je obavezan.")]
        [ForeignKey("Profesor")]
        public long ProfesorId { get; set; }
        public Profesor Profesor { get; set; }
    }
}