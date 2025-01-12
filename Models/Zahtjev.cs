using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public enum TipZahtjeva
    {
        [Display(Name = "Upis")]
        Upis,
        [Display(Name = "Statusna potvrda")]
        StatusnaPotvrda,
        [Display(Name = "Ispitna potvrda")]
        IspitnaPotvrda,
        [Display(Name = "Ostalo")]
        Ostalo
    }

    public enum StatusZahtjeva
    {
        [Display(Name = "Podnesen")]
        Podnesen,
        [Display(Name = "Odbijen")]
        Odbijen,
        [Display(Name = "Prihvaćen")]
        Prihvaćen
    }

    public class Zahtjev
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Tip zahtjeva je obavezan.")]
        [EnumDataType(typeof(TipZahtjeva))]
        public TipZahtjeva TipZahtjeva { get; set; }

        [Required(ErrorMessage = "Status zahtjeva je obavezan.")]
        [EnumDataType(typeof(StatusZahtjeva))]
        public StatusZahtjeva StatusZahtjeva { get; set; }

        [Required(ErrorMessage = "Datum podnošenja je obavezan.")]
        public DateTime DatumPodnosenja { get; set; }

        public DateTime? DatumRjesavanja { get; set; }

        [Required(ErrorMessage = "Student je obavezan.")]
        [ForeignKey("Student")]
        public long StudentId { get; set; }
        public Student Student { get; set; }

        public Zahtjev() { }
    }
}