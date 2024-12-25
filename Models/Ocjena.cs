using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class Ocjena
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [Range(5, 10)]
        public float Vrijednost { get; set; }

        [Required]
        [ForeignKey("Predmet")]
        public long PredmetId { get; set; }
        public Predmet Predmet { get; set; }

        [Required]
        [ForeignKey("Student")]
        public long brojIndeksa { get; set; }
        public Student Student { get; set; }

        [Required]
        [ForeignKey("Profesor")]
        public long ProfesorId { get; set; }
        public Profesor Profesor { get; set; }

        public Ocjena() { }
    }
}