using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class TerminNastave
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [ForeignKey("Predmet")]
        public long PredmetId { get; set; }
        public Predmet? Predmet { get; set; }

        [Required]
        public VrstaNastave Vrsta { get; set; }

        [Required]
        public DayOfWeek Dan { get; set; }

        [Required]
        public TimeSpan VrijemeOd { get; set; }

        [Required]
        public TimeSpan VrijemeDo { get; set; }

        [Required]
        [StringLength(50)]
        public string Lokacija { get; set; }

        [ForeignKey("Raspored")]
        public long? RasporedId { get; set; }
        public Raspored? Raspored { get; set; }
    }
}
