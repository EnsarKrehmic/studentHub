using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace StudentHub.Models
{
    public class PredmetAsistent
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Predmet je obavezan.")]
        [ForeignKey("Predmet")]
        public long PredmetId { get; set; }
        public Predmet Predmet { get; set; }

        [Required(ErrorMessage = "Asistent je obavezan.")]
        [ForeignKey("Asistent")]
        public long AsistentId { get; set; }
        public Asistent Asistent { get; set; }
    }
}
