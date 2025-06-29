using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class Raspored
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Studijski program je obavezan.")]
        public long StudijskiProgramId { get; set; }

        public StudijskiProgram? StudijskiProgram { get; set; }

        [Required]
        [Range(1, 6)]
        public int GodinaStudija { get; set; }

        [Required]
        [Range(1, 2)]
        public int Semestar { get; set; } // 1 = zimski, 2 = ljetni

        [Required]
        [StringLength(20)]
        public string AkademskaGodina { get; set; }

        public List<TerminNastave> Termini { get; set; } = new();
    }
}
