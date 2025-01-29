using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class Obavjestenje
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Naslov obavještenja je obavezan.")]
        [MaxLength(200, ErrorMessage = "Naslov ne smije biti duži od 200 karaktera.")]
        public string Naslov { get; set; }

        [Required(ErrorMessage = "Sadržaj obavještenja je obavezan.")]
        [MaxLength(1000, ErrorMessage = "Sadržaj ne smije biti duži od 1000 karaktera.")]
        public string Sadrzaj { get; set; }

        [Required]
        public DateTime DatumObjave { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Studijski program je obavezan.")]
        [ForeignKey("StudijskiProgram")]
        public long StudijskiProgramId { get; set; }
        public StudijskiProgram StudijskiProgram { get; set; }

        [ForeignKey("Korisnik")]
        public long? KorisnikId { get; set; }
        public Korisnik Korisnik { get; set; }

        [ForeignKey("StudentskaSluzba")]
        public long? StudentskaSluzbaId { get; set; }
        public StudentskaSluzba StudentskaSluzba { get; set; }

        [ForeignKey("Profesor")]
        public long? ProfesorId { get; set; }
        public Profesor Profesor { get; set; }

        [ForeignKey("Asistent")]
        public long? AsistentId { get; set; }
        public Asistent Asistent { get; set; }

        public Obavjestenje() { }
    }
}