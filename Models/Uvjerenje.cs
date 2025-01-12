using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public enum VrstaUvjerenja
    {
        [Display(Name = "Regularno uvjerenje")]
        Regularno,
        [Display(Name = "Diplomsko uvjerenje")]
        Diplomsko,
        [Display(Name = "Ostalo uvjerenje")]
        Ostalo
    }

    public class Uvjerenje
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Namjena je obavezna.")]
        [MaxLength(200, ErrorMessage = "Namjena može sadržavati najviše 200 karaktera.")]
        public string Namjena { get; set; }

        [Required(ErrorMessage = "Datum izdavanja je obavezan.")]
        public DateTime DatumIzdavanja { get; set; }

        [Required(ErrorMessage = "Student je obavezan.")]
        [ForeignKey("Student")]
        public long StudentId { get; set; }
        public Student Student { get; set; }

        [Required(ErrorMessage = "Studentska služba je obavezna.")]
        [ForeignKey("StudentskaSluzba")]
        public long StudentskaSluzbaId { get; set; }
        public StudentskaSluzba StudentskaSluzba { get; set; }

        [Required(ErrorMessage = "Vrsta uvjerenja je obavezna.")]
        [EnumDataType(typeof(VrstaUvjerenja))]
        public VrstaUvjerenja Vrsta { get; set; }

        public Uvjerenje() { }
    }
}