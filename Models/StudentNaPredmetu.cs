using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class StudentNaPredmetu
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string akademskaGodina { get; set; }

        [Required]
        [ForeignKey("Student")]
        public long StudentId { get; set; }
        public Student Student { get; set; }

        [Required]
        [ForeignKey("Predmet")]
        public long PredmetId { get; set; }
        public Predmet Predmet { get; set; }
    }
}