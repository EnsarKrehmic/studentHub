using StudentHub.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentHub.ViewModels
{
    public class ProfesorCreateViewModel
    {
        // Identity podaci
        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Unesite validnu email adresu.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Lozinka je obavezna.")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Lozinka mora imati najmanje {2} karaktera.", MinimumLength = 6)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Potvrda lozinke je obavezna.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Lozinka i potvrda lozinke se ne podudaraju.")]
        public string ConfirmPassword { get; set; }

        // Poslovni podaci
        [StringLength(13, MinimumLength = 13, ErrorMessage = "JMBG mora imati tačno 13 karaktera.")]
        public string? JMBG { get; set; }

        [MaxLength(50, ErrorMessage = "Ime ne može biti duže od 50 karaktera.")]
        public string? Ime { get; set; }

        [MaxLength(50, ErrorMessage = "Prezime ne može biti duže od 50 karaktera.")]
        public string? Prezime { get; set; }

        public string? ProfesorTitula { get; set; }

        [Required(ErrorMessage = "Studijski program je obavezan.")]
        [Display(Name = "Studijski program/i")]
        public List<long> StudijskiProgramIds { get; set; } = new List<long>();

        [Display(Name = "Predmet/i")]
        public List<long> PredmetIds { get; set; } = new List<long>();
    }
}
