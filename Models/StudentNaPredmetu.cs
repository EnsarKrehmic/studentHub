using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class StudentNaPredmetu
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Akademska godina je obavezna.")]
        [MaxLength(50, ErrorMessage = "Akademska godina ne može biti duža od 50 karaktera.")]
        public string AkademskaGodina { get; set; }

        [Required(ErrorMessage = "Student je obavezan.")]
        [ForeignKey("Student")]
        public long StudentId { get; set; }
        public Student Student { get; set; }

        [Required(ErrorMessage = "Predmet je obavezan.")]
        [ForeignKey("Predmet")]
        public long PredmetId { get; set; }
        public Predmet Predmet { get; set; }
    }
}