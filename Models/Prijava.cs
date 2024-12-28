using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class Prijava
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Datum prijave je obavezan.")]
        public DateTime datumPrijave { get; set; }

        [Required(ErrorMessage = "Ispit je obavezan.")]
        [ForeignKey("Ispit")]
        public long IspitId { get; set; }
        public Ispit Ispit { get; set; }

        [Required(ErrorMessage = "Student je obavezan.")]
        [ForeignKey("Student")]
        public long StudentId { get; set; }
        public Student Student { get; set; }

        public Prijava() { }
    }
}