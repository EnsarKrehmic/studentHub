using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class PrisustvoNaAktivnosti
    {
        public long Id { get; set; }

        public long StudentId { get; set; }
        public Student Student { get; set; }

        public long NastavnaAktivnostId { get; set; }
        public NastavnaAktivnost NastavnaAktivnost { get; set; }

        public DateTime VrijemeEvidentiranja { get; set; } = DateTime.Now;
    }
}
