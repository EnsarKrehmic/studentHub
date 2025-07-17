using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class PodrskaUpit
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Naslov je obavezan.")]
        [StringLength(100)]
        public string Naslov { get; set; }

        [Required(ErrorMessage = "Opis upita je obavezan.")]
        [StringLength(2000)]
        public string Opis { get; set; }

        public DateTime DatumKreiranja { get; set; } = DateTime.Now;

        public UpitStatus Status { get; set; } = UpitStatus.Podnesen;

        // FK prema korisniku koji je postavio upit
        [Required]
        public long KorisnikId { get; set; }
        [ForeignKey("KorisnikId")]
        public Korisnik Korisnik { get; set; }

        // Odgovor (opcionalno)
        public string? Odgovor { get; set; }

        public DateTime? DatumOdgovora { get; set; }
    }

    public enum UpitStatus
    {
        Podnesen = 0,
        UObradi = 1,
        Zatvoren = 2
    }
}
