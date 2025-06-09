using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class NastavniMaterijal
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Naziv je obavezan.")]
        [MaxLength(100, ErrorMessage = "Naziv ne može biti duži od 100 karaktera.")]
        public string Naziv { get; set; }

        [MaxLength(500, ErrorMessage = "Opis ne može biti duži od 500 karaktera.")]
        public string Opis { get; set; }

        public string PutanjaDoFajla { get; set; }

        public string TipFajla { get; set; }

        [ForeignKey("NastavnaAktivnost")]
        public long NastavnaAktivnostId { get; set; }
        public NastavnaAktivnost NastavnaAktivnost { get; set; }
    }
}