using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class StudijskiProgramIzborniLimit
    {
        [Key]
        public long Id { get; set; }

        [ForeignKey("StudijskiProgram")]
        public long StudijskiProgramId { get; set; }

        [BindNever]
        public StudijskiProgram? StudijskiProgram { get; set; }

        [Required(ErrorMessage = "Godina studija je obavezna.")]
        [Range(1, 6, ErrorMessage = "Godina studija mora biti između 1 i 6.")]
        public int GodinaStudija { get; set; }

        [Required(ErrorMessage = "Minimalan broj izbornih predmeta je obavezan.")]
        [Range(0, 10, ErrorMessage = "Minimalan broj izbornih predmeta mora biti između 0 i 10.")]
        public int MinIzborniPredmeti { get; set; }

        [Required(ErrorMessage = "Maksimalan broj izbornih predmeta je obavezan.")]
        [Range(0, 10, ErrorMessage = "Maksimalan broj izbornih predmeta mora biti između 0 i 10.")]
        public int MaxIzborniPredmeti { get; set; }
    }
}
