using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class ZahtjevZaPrisustvo
    {
        public long Id { get; set; }

        public long StudentId { get; set; }
        public Student Student { get; set; }

        public long NastavnaAktivnostId { get; set; }
        public NastavnaAktivnost NastavnaAktivnost { get; set; }

        public DateTime VrijemePodnosenja { get; set; } = DateTime.Now;

        public bool Odbijen { get; set; } = false;

        public string KodUnesen { get; set; }
    }
}