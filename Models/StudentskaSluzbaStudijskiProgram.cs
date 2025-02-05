using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace StudentHub.Models
{
    public class StudentskaSluzbaStudijskiProgram
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Studentska služba je obavezan.")]
        [ForeignKey("StudentskaSluzba")]
        public long StudentskaSluzbaId { get; set; }
        public StudentskaSluzba StudentskaSluzba { get; set; }

        [Required(ErrorMessage = "Studijski program je obavezan.")]
        [ForeignKey("StudijskiProgram")]
        public long StudijskiProgramId { get; set; }
        public StudijskiProgram StudijskiProgram { get; set; }
    }
}
