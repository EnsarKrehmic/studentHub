using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
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

        [Display(Name = "Datum i vrijeme")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime DatumVrijeme { get; set; } = DateTime.Now;

        // Autor komentara: ili Korisnik (prof/asist), ili Student
        [ForeignKey(nameof(Korisnik))]
        public long? KorisnikId { get; set; }
        public Korisnik? Korisnik { get; set; }

        [ForeignKey(nameof(Student))]
        public long? StudentId { get; set; }
        public Student? Student { get; set; }

        // Ako je komentar na ispit
        [ForeignKey(nameof(Ispit))]
        public long? IspitId { get; set; }
        public Ispit? Ispit { get; set; }

        // Ako je komentar na nastavnu aktivnost
        [ForeignKey(nameof(NastavnaAktivnost))]
        public long? NastavnaAktivnostId { get; set; }
        public NastavnaAktivnost? NastavnaAktivnost { get; set; }

        public VidljivostKomentara Vidljivost { get; set; } = VidljivostKomentara.Javno;

        public string? PrilogPath { get; set; }

        // Mogućnost označavanja drugog korisnika
        public string? MentionedUserId { get; set; }
        [ForeignKey(nameof(MentionedUserId))]
        public IdentityUser? MentionedUser { get; set; }

        // Dodatna kontrola tko sve može vidjeti privatni komentar
        public ICollection<KomentarVidljivost> VidljivostKorisnici { get; set; }
            = new List<KomentarVidljivost>();

        // Pomoćna svojstva za prikaz u View‐u
        [NotMapped]
        public string AutorDisplayName =>
            Student != null
                ? $"{Student.Ime} {Student.Prezime}"
                : Korisnik != null
                    ? $"{Korisnik.Ime} {Korisnik.Prezime}"
                    : "Nepoznat autor";

        [NotMapped]
        public string PovezanaEntitetDisplay =>
            NastavnaAktivnost != null
                ? NastavnaAktivnost.Naziv
                : Ispit != null
                    ? $"{Ispit.Predmet?.Naziv} ({Ispit.DatumOdrzavanja:dd.MM.yyyy})"
                    : "N/A";
    }

    public enum VidljivostKomentara
    {
        [Display(Name = "Javno")]
        Javno,
        [Display(Name = "Privatno")]
        Privatno
    }

    public class KomentarVidljivost
    {
        public long KomentarId { get; set; }
        public Komentar Komentar { get; set; } = default!;

        public long KorisnikId { get; set; }
        public Korisnik Korisnik { get; set; } = default!;
    }
}
