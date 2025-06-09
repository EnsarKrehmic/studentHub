using StudentHub.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentHub.ViewModels
{
    public class AsistentCreateViewModel
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
        [Required(ErrorMessage = "Ime je obavezno.")]
        public string Ime { get; set; }

        [Required(ErrorMessage = "Prezime je obavezno.")]
        public string Prezime { get; set; }

        [Required(ErrorMessage = "JMBG je obavezan.")]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "JMBG mora imati tačno 13 karaktera.")]
        public string JMBG { get; set; }

        [Required(ErrorMessage = "Titula je obavezna.")]
        [Display(Name = "Titula")]
        public string AsistentTitula { get; set; }

        public List<long> StudijskiProgramIds { get; set; } = new List<long>();

        public List<long> PredmetIds { get; set; } = new List<long>();
    }
}
