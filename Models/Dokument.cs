using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class Dokument
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Naziv dokumenta je obavezan.")]
        [MaxLength(200, ErrorMessage = "Naziv dokumenta ne smije biti duži od 200 karaktera.")]
        public string Naziv { get; set; }

        [Required(ErrorMessage = "Student je obavezan.")]
        [ForeignKey("Student")]
        public long StudentId { get; set; }
        public Student Student { get; set; }

        [Required(ErrorMessage = "Studentska služba je obavezna.")]
        [ForeignKey("StudentskaSluzba")]
        public long StudentskaSluzbaId { get; set; }
        public StudentskaSluzba StudentskaSluzba { get; set; }
        public List<DokumentSlike> Slike { get; set; }

        public Dokument() { }
    }
}
