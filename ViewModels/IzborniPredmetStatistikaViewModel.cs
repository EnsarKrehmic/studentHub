namespace StudentHub.ViewModels
{
    public class IzborniPredmetStatistikaGroupedViewModel
    {
        public string StudijskiProgramNaziv { get; set; }
        public List<IzborniPredmetStatistikaGodinaViewModel> GodineStudija { get; set; } = new();
    }

    public class IzborniPredmetStatistikaGodinaViewModel
    {
        public int GodinaStudija { get; set; }
        public List<IzborniPredmetStatistikaViewModel> Statistika { get; set; } = new();
    }

    public class IzborniPredmetStatistikaViewModel
    {
        public string NazivPredmeta { get; set; }
        public int BrojStudenata { get; set; }
        public double Procenat { get; set; }
    }
}
