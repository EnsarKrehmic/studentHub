using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public enum TipNastavneAktivnosti
    {
        Predavanje,
        Vjezba
    }

    public class NastavnaAktivnost
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Naziv je obavezan.")]
        [MaxLength(100, ErrorMessage = "Naziv ne može biti duži od 100 karaktera.")]
        public string Naziv { get; set; }

        [StringLength(500, ErrorMessage = "Opis ne smije biti duži od 500 znakova")]
        [DataType(DataType.MultilineText)]
        public string Opis { get; set; }

        [Required(ErrorMessage = "Tip aktivnosti je obavezan.")]
        public TipNastavneAktivnosti Tip { get; set; }

        [Required(ErrorMessage = "Datum i vrijeme održavanja su obavezni.")]
        [DisplayName("Datum i vrijeme održavanja")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm}")]
        public DateTime DatumVrijemeOdrzavanja { get; set; }

        [DisplayName("Postavi kao dostupno")]
        public bool ManuelnoOtkljucano { get; set; } = false;

        [DisplayName("Postavi kao nedostupno")]
        public bool ManuelnoZakljucano { get; set; } = false;

        [DisplayName("Kod za prisustvo")]
        public string? KodZaPrisustvo { get; set; }

        [DisplayName("Vrijeme generisanja koda")]
        public DateTime? VrijemeGenerisanjaKoda { get; set; }

        [DisplayName("Kod važi do")]
        public DateTime? KodAktivanDo { get; set; }

        [ForeignKey("Predmet")]
        public long PredmetId { get; set; }
        public Predmet Predmet { get; set; }

        public List<NastavniMaterijal> NastavniMaterijali { get; set; } = new();
        public List<Komentar> Komentari { get; set; } = new();
        public List<Ocjena> Ocjene { get; set; } = new();
        public List<PrisustvoNaAktivnosti> Prisustva { get; set; } = new();

        // Svojstvo za prosječnu ocjenu
        public double ProsjecnaOcjena => Ocjene.Any() ? Ocjene.Average(o => o.Vrijednost) : 0;

        // Logika za provjeru dostupnosti sadržaja
        [DisplayName("Dostupno")]
        public bool JeDostupno => (ManuelnoOtkljucano && !ManuelnoZakljucano) ||
                                  (DateTime.Now >= DatumVrijemeOdrzavanja && !ManuelnoZakljucano);
    }
}
