using StudentHub.Models;
using System.ComponentModel.DataAnnotations;

namespace StudentHub.ViewModels
{
    public class StudentEditViewModel
    {
        public const string PasswordSentinel = "********";

        public long Id { get; set; }

        [StringLength(13, MinimumLength = 13, ErrorMessage = "JMBG mora imati tačno 13 karaktera.")]
        [Display(Name = "JMBG")]
        public string? JMBG { get; set; }

        [MaxLength(50, ErrorMessage = "Ime ne može biti duže od 50 karaktera.")]
        [Display(Name = "Ime")]
        public string? Ime { get; set; }

        [MaxLength(50, ErrorMessage = "Prezime ne može biti duže od 50 karaktera.")]
        [Display(Name = "Prezime")]
        public string? Prezime { get; set; }

        [EmailAddress(ErrorMessage = "Unesite validnu email adresu.")]
        [Display(Name = "E-mail")]
        public string? Email { get; set; }

        [MaxLength(20, ErrorMessage = "Broj indeksa ne može biti duži od 20 karaktera.")]
        [Display(Name = "Broj indeksa")]
        public string? BrojIndeksa { get; set; }

        [MaxLength(200, ErrorMessage = "Prethodno obrazovanje ne može biti duže od 200 karaktera.")]
        [Display(Name = "Prethodno obrazovanje")]
        public string? PrethodnoObrazovanje { get; set; }

        [Required(ErrorMessage = "Godina studija je obavezna.")]
        [Range(1, 6, ErrorMessage = "Godina studija mora biti između 1 i 6.")]
        [Display(Name = "Godina studija")]
        public int? GodinaStudija { get; set; }

        [Required(ErrorMessage = "Semestar je obavezan.")]
        [Range(1, 2, ErrorMessage = "Semestar mora biti 1 ili 2.")]
        [Display(Name = "Semestar")]
        public int? Semestar { get; set; }

        [Required(ErrorMessage = "Studijski program je obavezan.")]
        [Display(Name = "Studijski program")]
        public long StudijskiProgramId { get; set; }

        [Display(Name = "Nastavni plan")]
        public long? NastavniPlanId { get; set; }

        public bool IzborIzbornihPredmetaZakljucan { get; set; }

        [Display(Name = "Predmeti")]
        public List<long> PredmetIds { get; set; } = new List<long>();

        [Required(ErrorMessage = "Uloga je obavezna.")]
        [Display(Name = "Uloga")]
        public Uloga Uloga { get; set; }

        // --- Lozinka (opciono) ---
        [DataType(DataType.Password)]
        [Display(Name = "Nova lozinka")]
        [StringLength(100, ErrorMessage = "Lozinka mora imati najmanje {2} karaktera.", MinimumLength = 6)]
        public string? NewPassword { get; set; } = PasswordSentinel;

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Nova lozinka i potvrda se ne podudaraju.")]
        [Display(Name = "Potvrdi novu lozinku")]
        public string? ConfirmNewPassword { get; set; } = PasswordSentinel;

        [Display(Name = "Želim promijeniti lozinku")]
        public bool ChangePassword { get; set; } = false;
    }
}
