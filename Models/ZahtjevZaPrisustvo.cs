using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class ZahtjevZaPrisustvo
    {
        [Key]
        public long Id { get; set; }

        [ForeignKey("Student")]
        public long StudentId { get; set; }
        public Student Student { get; set; }

        [ForeignKey("NastavnaAktivnost")]
        public long NastavnaAktivnostId { get; set; }
        public NastavnaAktivnost NastavnaAktivnost { get; set; }

        [Required]
        public DateTime VrijemePodnosenja { get; set; } = DateTime.Now;

        [Required]
        public string KodUnesen { get; set; }

        public bool Obradjen { get; set; } = false;

        public bool? Odbijen { get; set; } = null;

        public string? Napomena { get; set; }

        [NotMapped]
        public Predmet Predmet => NastavnaAktivnost?.Predmet;
    }
}
