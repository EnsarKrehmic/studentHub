using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace StudentHub.Models
{
    public class PredmetAsistent
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [ForeignKey("Predmet")]
        public long PredmetId { get; set; }
        public Predmet Predmet { get; set; }

        [Required]
        [ForeignKey("Asistent")]
        public long AsistentId { get; set; }
        public Asistent Asistent { get; set; }
    }
}
