using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class Obavjestenje
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Naslov { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Sadrzaj { get; set; }

        [Required]
        public DateTime datumObjave { get; set; } = DateTime.Now;

        public long? KorisnikId { get; set; }
        public Korisnik Korisnik { get; set; }

        public long? StudentskaSluzbaId { get; set; }
        public StudentskaSluzba StudentskaSluzba { get; set; }

        public long? ProfesorId { get; set; }
        public Profesor Profesor { get; set; }

        public long? AsistentId { get; set; }
        public Asistent Asistent { get; set; }

        public Obavjestenje() { }
    }
}