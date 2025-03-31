namespace StudentHub.ViewModels
{
    public class OcjenaViewModel
    {
        public long Id { get; set; }
        public string Tip { get; set; }
        public string PredmetNaziv { get; set; }
        public string NastavnaAktivnostNaziv { get; set; }
        public string StudentIme { get; set; }
        public string StudentPrezime { get; set; }
        public string StudentBrojIndeksa { get; set; }
        public string ProfesorIme { get; set; }
        public string ProfesorPrezime { get; set; }
        public string ProfesorTitula { get; set; }
        public float Vrijednost { get; set; }
        public double ProsjekOcjena { get; set; }
        public double ProsjekPoPredmetu { get; set; }
        public double ProsjekPoStudijskomProgramu { get; set; }
        public string ProsjekPrikaz { get; set; }
        public string StudentStudijskiProgramNaziv { get; set; }
        public long PredmetId { get; set; }
        public long StudijskiProgramId { get; set; }
    }
}