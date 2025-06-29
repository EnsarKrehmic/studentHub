namespace StudentHub.ViewModels
{
    public class MojiPodaciIndexViewModel
    {
        public string Uloga { get; set; }
        public long KorisnikId { get; set; }

        // Studentska služba
        public int BrojNerijesenihZahtjeva { get; set; }

        // Profesor/Asistent
        public int BrojZahtjevaZaPrisustvo { get; set; }
    }
}
