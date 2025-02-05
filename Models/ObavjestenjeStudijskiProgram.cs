using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class ObavjestenjeStudijskiProgram
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Obavještenje je obavezno.")]
        [ForeignKey("Obavjestenje")]
        public long ObavjestenjeId { get; set; }
        public Obavjestenje Obavjestenje { get; set; }

        [Required(ErrorMessage = "Studijski program je obavezan.")]
        [ForeignKey("StudijskiProgram")]
        public long StudijskiProgramId { get; set; }
        public StudijskiProgram StudijskiProgram { get; set; }
    }
}
