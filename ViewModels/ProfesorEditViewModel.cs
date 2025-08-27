using StudentHub.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentHub.ViewModels
{
    public class ProfesorEditViewModel
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

        [Display(Name = "Titula")]
        public string? ProfesorTitula { get; set; }

        [Display(Name = "Studijski program/i")]
        public List<long> StudijskiProgramIds { get; set; } = new List<long>();

        [Display(Name = "Predmet/i")]
        public List<long> PredmetIds { get; set; } = new List<long>();

        [Required(ErrorMessage = "Uloga je obavezna.")]
        [EnumDataType(typeof(Uloga))]
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
