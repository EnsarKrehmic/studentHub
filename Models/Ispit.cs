using System.ComponentModel.DataAnnotations;

namespace StudentHub.Models
{
    public class Ispit
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Datum održavanja je obavezan.")]
        public DateTime datumOdrzavanja { get; set; }

        [Required]
        public DateTime datumObjave { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Lokacija je obavezna.")]
        [MaxLength(200, ErrorMessage = "Lokacija ne smije biti duža od 200 karaktera.")]
        public string Lokacija { get; set; }

        [Range(0, 100, ErrorMessage = "Broj bodova mora biti između 0 i 100.")]
        public int brojBodova { get; set; }

        [Required(ErrorMessage = "Predmet je obavezan.")]
        public long PredmetId { get; set; }
        public Predmet Predmet { get; set; }

        public long? KorisnikId { get; set; }
        public Korisnik? Korisnik { get; set; }

        public long? ProfesorId { get; set; }
        public Profesor? Profesor { get; set; }

        public long? AsistentId { get; set; }
        public Asistent? Asistent { get; set; }

        public long? StudentId { get; set; }
        public Student? Student { get; set; }

        public Ispit() { }
    }
}
