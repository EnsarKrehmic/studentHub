using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public enum TipOcjene
    {
        Predmet,
        NastavnaAktivnost
    }

    public class Ocjena
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public TipOcjene Tip { get; set; }

        [Required(ErrorMessage = "Ocjena je obavezna.")]
        public float Vrijednost { get; set; }

        // Validacija raspona ovisno o tipu ocjene
        public bool IsValid()
        {
            if (Tip == TipOcjene.Predmet && (Vrijednost < 5 || Vrijednost > 10))
                return false;
            if (Tip == TipOcjene.NastavnaAktivnost && (Vrijednost < 1 || Vrijednost > 5))
                return false;
            return true;
        }

        [ForeignKey("Predmet")]
        public long? PredmetId { get; set; }
        public Predmet Predmet { get; set; }

        [ForeignKey("Student")]
        public long StudentId { get; set; }
        public Student Student { get; set; }

        [ForeignKey("Profesor")]
        public long? ProfesorId { get; set; }
        public Profesor Profesor { get; set; }

        [ForeignKey("NastavnaAktivnost")]
        public long? NastavnaAktivnostId { get; set; }
        public NastavnaAktivnost NastavnaAktivnost { get; set; }

        public Ocjena() { }
    }
}