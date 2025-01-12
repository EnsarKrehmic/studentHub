using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class NastavniPlan
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Godina studija je obavezna.")]
        [MaxLength(100, ErrorMessage = "Godina studija ne smije biti duža od 100 karaktera.")]
        public string GodinaStudija { get; set; }

        [Required(ErrorMessage = "Studijski program je obavezan.")]
        [ForeignKey("StudijskiProgram")]
        public long StudijskiProgramId { get; set; }
        public StudijskiProgram StudijskiProgram { get; set; }

        public NastavniPlan() { }
    }
}