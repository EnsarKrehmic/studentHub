using System.ComponentModel.DataAnnotations;

namespace StudentHub.ViewModels
{
    public class GenerisanjeTerminaViewModel
    {
        public long RasporedId { get; set; }

        [Display(Name = "Dani za raspoređivanje")]
        public List<DayOfWeek> Dani { get; set; } = new();

        [Display(Name = "Početni sat")]
        public int SatOd { get; set; } = 8;

        [Display(Name = "Zadnji sat")]
        public int SatDo { get; set; } = 18;

        [Display(Name = "Trajanje termina (minuta)")]
        public int TrajanjeMin { get; set; } = 45;

        // NOVO
        [Display(Name = "Broj sedmica u semestru")]
        [Range(1, 30, ErrorMessage = "Broj sedmica mora biti između 1 i 30.")]
        public int BrojSedmica { get; set; } = 15;

        [Display(Name = "Pauza između termina (minuta)")]
        public int PauzaMin { get; set; } = 0;

        [Display(Name = "Dostupne lokacije (učionice)")]
        public List<string> SveLokacije { get; set; } = new();

        [Display(Name = "Odabrane lokacije")]
        public List<string> OdabraneLokacije { get; set; } = new();

        public bool NeRasporedjujPetkomPopodne { get; set; } = false;
        public bool IzbjegavajUzastopneTermine { get; set; } = false;
    }
}
