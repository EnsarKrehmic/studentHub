using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class Prijava
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public DateTime datumPrijave { get; set; }

        [Required]
        [ForeignKey("Ispit")]
        public long IspitId { get; set; }
        public Ispit Ispit { get; set; }

        [Required]
        [ForeignKey("Student")]
        public long StudentId { get; set; }
        public Student Student { get; set; }

        public Prijava() { }
    }
}