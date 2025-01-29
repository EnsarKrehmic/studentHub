namespace StudentHub.ViewModels
{
    public class OcjeneViewModel
    {
        public long StudentId { get; set; }
        public string StudentIme { get; set; }
        public string StudentPrezime { get; set; }
        public List<OcjenaPredmetViewModel> Ocjene { get; set; }
        public float Prosjek { get; set; }
    }

    public class OcjenaPredmetViewModel
    {
        public string PredmetNaziv { get; set; }
        public float OcjenaVrijednost { get; set; }
    }
}