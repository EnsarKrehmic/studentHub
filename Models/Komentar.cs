using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class Komentar
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Sadržaj komentara je obavezan.")]
        public string Sadrzaj { get; set; }

        [Required(ErrorMessage = "Datum i vrijeme su obavezni.")]
        public DateTime DatumVrijeme { get; set; }

        [ForeignKey("Student")]
        public long StudentId { get; set; }
        public Student Student { get; set; }

        [ForeignKey("NastavnaAktivnost")]
        public long NastavnaAktivnostId { get; set; }
        public NastavnaAktivnost NastavnaAktivnost { get; set; }
    }
}
