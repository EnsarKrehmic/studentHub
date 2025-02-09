using StudentHub.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentHub.ViewModels
{
    public class ProfesorCreateViewModel
    {
        [Required(ErrorMessage = "Ime je obavezno.")]
        public string Ime { get; set; }

        [Required(ErrorMessage = "Prezime je obavezno.")]
        public string Prezime { get; set; }

        [Required(ErrorMessage = "JMBG je obavezan.")]
        public string JMBG { get; set; }

        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Unesite validnu email adresu.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Lozinka je obavezna.")]
        [DataType(DataType.Password)]
        public string Lozinka { get; set; }

        [Required(ErrorMessage = "Titula je obavezna.")]
        [Display(Name = "Titula")]
        public string ProfesorTitula { get; set; }

        [Required(ErrorMessage = "Uloga je obavezna.")]
        [EnumDataType(typeof(Uloga))]
        public Uloga Uloga { get; set; }

        public List<long> StudijskiProgramIds { get; set; } = new List<long>();
        public List<long> PredmetIds { get; set; } = new List<long>();
    }
}
