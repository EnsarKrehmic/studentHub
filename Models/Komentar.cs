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

        public DateTime DatumVrijeme { get; set; } = DateTime.Now;

        // Veza sa korisnikom (Profesor/Asistent/Student) koji je autor
        [ForeignKey("Korisnik")]
        public long KorisnikId { get; set; }
        public Korisnik? Korisnik { get; set; }

        [ForeignKey("Student")]
        public long? StudentId { get; set; }
        public Student? Student { get; set; }

        [ForeignKey("Ispit")]
        public long? IspitId { get; set; }
        public Ispit? Ispit { get; set; }

        public VidljivostKomentara Vidljivost { get; set; } = VidljivostKomentara.Javno;

        public string? PrilogPath { get; set; }

        [ForeignKey("NastavnaAktivnost")]
        public long? NastavnaAktivnostId { get; set; }
        public NastavnaAktivnost? NastavnaAktivnost { get; set; }

        public string? MentionedUserId { get; set; }

        [ForeignKey("MentionedUserId")]
        public IdentityUser? MentionedUser { get; set; }

        public ICollection<KomentarVidljivost> VidljivostKorisnici { get; set; } = new List<KomentarVidljivost>();
    }

    public enum VidljivostKomentara
    {
    [Display(Name = "Javno")]
    Javno,
    [Display(Name = "Privatno")]
    Privatno
    }

    // Nova klasa za mapiranje vidljivosti
    public class KomentarVidljivost
    {
    public long KomentarId { get; set; }
    public Komentar Komentar { get; set; }

    public long KorisnikId { get; set; }
    public Korisnik Korisnik { get; set; }
    }
}
