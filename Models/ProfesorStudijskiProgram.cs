using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class ProfesorStudijskiProgram
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Profesor je obavezan.")]
        [ForeignKey("Profesor")]
        public long ProfesorId { get; set; }
        public Profesor Profesor { get; set; }

        [Required(ErrorMessage = "Studijski program je obavezan.")]
        [ForeignKey("StudijskiProgram")]
        public long StudijskiProgramId { get; set; }
        public StudijskiProgram StudijskiProgram { get; set; }
    }
}