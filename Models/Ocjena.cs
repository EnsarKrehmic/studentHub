using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class Ocjena
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Vrijednost ocjene je obavezna.")]
        [Range(5, 10, ErrorMessage = "Vrijednost ocjene mora biti između 5 i 10.")]
        public float Vrijednost { get; set; }

        [Required(ErrorMessage = "ID predmeta je obavezan.")]
        [ForeignKey("Predmet")]
        public long PredmetId { get; set; }
        public Predmet Predmet { get; set; }

        [Required(ErrorMessage = "Student je obavezan.")]
        [ForeignKey("Student")]
        public long StudentId { get; set; }
        public Student Student { get; set; }

        [Required(ErrorMessage = "Profesor je obavezan.")]
        [ForeignKey("Profesor")]
        public long ProfesorId { get; set; }
        public Profesor Profesor { get; set; }

        public Ocjena() { }
    }
}