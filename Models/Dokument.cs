using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class Dokument
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Naziv { get; set; }

        [Required]
        [MaxLength(500)]
        public string Putanja { get; set; }

        [Required]
        [ForeignKey("Student")]
        public long brojIndeksa { get; set; }
        public Student Student { get; set; }

        [Required]
        [ForeignKey("StudentskaSluzba")]
        public long StudentskaSluzbaId { get; set; }
        public StudentskaSluzba StudentskaSluzba { get; set; }

        public Dokument() { }
    }
}
