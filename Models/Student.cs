using StudentHub.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public class Student : Korisnik
    {
        [MaxLength(200)]
        public string predhodnoObrazovanje { get; set; }

        [Required]
        [MaxLength(200)]
        public string studijskiProgram { get; set; }

        [Required]
        [Range(1, 5)]
        public int godinaStudija { get; set; }

        public string podaciUplata { get; set; }

        public Student() { }
    }
}
