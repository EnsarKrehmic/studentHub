using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public enum BugTip
    {
        [Display(Name = "Greška u radu sistema")]
        Bug = 1,
        [Display(Name = "Prijedlog za poboljšanje")]
        Suggestion = 2
    }

    public enum BugStatus
    {
        [Display(Name = "Podnesen")]
        Podnesen = 1,
        [Display(Name = "U obradi")]
        UObradi = 2,
        [Display(Name = "Zatvoren")]
        Zatvoren = 3
    }

    public class BugReport
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Odaberite tip prijave.")]
        public BugTip Tip { get; set; }

        [Required(ErrorMessage = "Opis je obavezan.")]
        [MaxLength(2000)]
        public string Opis { get; set; }

        [MaxLength(200)]
        public string? Naslov { get; set; }

        public DateTime DatumPrijave { get; set; } = DateTime.Now;

        public BugStatus Status { get; set; } = BugStatus.Podnesen;

        // Korisnik koji prijavljuje
        [Required]
        public long KorisnikId { get; set; }
        [ForeignKey("KorisnikId")]
        public Korisnik Korisnik { get; set; }

        // Eventualni odgovor službe
        [MaxLength(2000)]
        public string? Odgovor { get; set; }
        public DateTime? DatumOdgovora { get; set; }

        // Opcionalno: naziv fajla slike/screenshot-a (putanja ili ime slike)
        [MaxLength(255)]
        public string? Slika { get; set; }
    }
}
