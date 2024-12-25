using System.ComponentModel.DataAnnotations;

namespace StudentHub.Models
{
    public enum Uloga
    {
        [Display(Name = "Studentska služba")]
        StudentskaSluzba = 1,
        [Display(Name = "Student")]
        Student = 2,
        [Display(Name = "Profesor")]
        Profesor = 3,
        [Display(Name = "Asistent")]
        Asistent = 4
    }
    public class Korisnik
    {
        [Key]
        public long JMBG { get; set; }

        [Required]
        [MaxLength(50)]
        public string Ime { get; set; }

        [Required]
        [MaxLength(50)]
        public string Prezime { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Lozinka { get; set; }

        [Required]
        [EnumDataType(typeof(Uloga))]
        public Uloga Uloga { get; set; }

        public Korisnik() { }
    }
}