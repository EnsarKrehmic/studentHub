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

        [Display(Name = "Datum unosa")]
        public DateTime DatumUnosa { get; set; } = DateTime.Now;

        [MaxLength(200)]
        [Display(Name = "Komentar ili opis")]
        public string? Komentar { get; set; }

        [Range(0, 100)]
        [Display(Name = "Težina (%)")]
        public int? TezinaProcentualno { get; set; }

        // Validacija raspona ovisno o tipu ocjene
        public bool IsValid()
        {
            if (Tip == TipOcjene.Predmet && (Vrijednost < 5 || Vrijednost > 10))
                return false;
            if (Tip == TipOcjene.NastavnaAktivnost && (Vrijednost < 1 || Vrijednost > 5))
                return false;
            return true;
        }

        public long? PredmetId { get; set; }
        public virtual Predmet? Predmet { get; set; }

        public long StudentId { get; set; }
        public virtual Student? Student { get; set; }

        public long? ProfesorId { get; set; }
        public virtual Profesor? Profesor { get; set; }

        public long? NastavnaAktivnostId { get; set; }
        public virtual NastavnaAktivnost? NastavnaAktivnost { get; set; }
        
        public long? IspitId { get; set; }
        public virtual Ispit? Ispit { get; set; }

        public long? ParentOcjenaId { get; set; }
        public virtual Ocjena? ParentOcjena { get; set; }

        public virtual ICollection<Ocjena> DjelimicneOcjene { get; set; } = new List<Ocjena>();
        public Ocjena() { }
    }
}