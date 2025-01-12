using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public enum Uloga
    {
        [Display(Name = "Osnovni")]
        Osnovni = 1,
        [Display(Name = "Studentska služba")]
        StudentskaSluzba = 2,
        [Display(Name = "Student")]
        Student = 3,
        [Display(Name = "Profesor")]
        Profesor = 4,
        [Display(Name = "Asistent")]
        Asistent = 5,
    }
    public class Korisnik
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "JMBG je obavezan.")]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "JMBG mora imati tačno 13 karaktera.")]
        public string JMBG { get; set; }

        [Required(ErrorMessage = "Ime je obavezno.")]
        [MaxLength(50, ErrorMessage = "Ime ne može biti duže od 50 karaktera.")]
        public string Ime { get; set; }

        [Required(ErrorMessage = "Prezime je obavezno.")]
        [MaxLength(50, ErrorMessage = "Prezime ne može biti duže od 50 karaktera.")]
        public string Prezime { get; set; }

        [EmailAddress(ErrorMessage = "Unesite validnu email adresu.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Lozinka je obavezna.")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Lozinka mora imati najmanje 8 karaktera.")]
        public string Lozinka { get; set; }

        [Required(ErrorMessage = "Uloga je obavezna.")]
        [EnumDataType(typeof(Uloga))]
        public Uloga Uloga { get; set; }
        public Korisnik() { }
    }
}