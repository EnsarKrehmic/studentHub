using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class StudentStudijskiProgram
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Student je obavezan.")]
        [ForeignKey("Student")]
        public long StudentId { get; set; }
        public Student Student { get; set; }

        [Required(ErrorMessage = "Studijski program je obavezan.")]
        [ForeignKey("StudijskiProgram")]
        public long StudijskiProgramId { get; set; }
        public StudijskiProgram StudijskiProgram { get; set; }
    }
}
