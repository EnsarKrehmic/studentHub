namespace StudentHub.ViewModels
{
    public class RezultatIspitaViewModel
    {
        public long StudentId { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Email { get; set; }
        public string BrojIndeksa { get; set; }
        public int Bodovi { get; set; }
        public int UslovZaPolaganje { get; set; }

        public bool Polozen => Bodovi >= UslovZaPolaganje;
    }
}
