using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public enum TipZahtjeva
    {
        [Display(Name = "Ispisnica")]
        Upis,
        [Display(Name = "Potrvda o statusu studenta")]
        StatusnaPotvrda,
        [Display(Name = "Uvjerenje o polozenim ispitima")]
        IspitnaPotvrda,
        [Display(Name = "Ostalo")]
        Ostalo
    }

    public enum StatusZahtjeva
    {
        [Display(Name = "Podnešen")]
        Podnešen,
        [Display(Name = "Primljen")]
        Primljen,
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
        [Display(Name = "Tip zahtjeva")]
        public TipZahtjeva TipZahtjeva { get; set; }

        [EnumDataType(typeof(StatusZahtjeva))]
        [Display(Name = "Status zahtjeva")]
        public StatusZahtjeva StatusZahtjeva { get; set; }

        [Display(Name = "Datum podnošenja")]
        public DateTime DatumPodnosenja { get; set; }

        [Display(Name = "Datum rješavanja")]
        public DateTime? DatumRjesavanja { get; set; }
        public string? Napomena { get; set; }

        [ForeignKey("Student")]
        public long StudentId { get; set; }
        public Student? Student { get; set; }

        public Zahtjev() { }
    }
}