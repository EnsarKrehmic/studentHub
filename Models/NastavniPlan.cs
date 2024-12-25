using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class NastavniPlan
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string godinaStudija { get; set; }

        [Required]
        [ForeignKey("StudijskiProgram")]
        public long StudijskiProgramId { get; set; }
        public StudijskiProgram StudijskiProgram { get; set; }

        public NastavniPlan() { }
    }
}