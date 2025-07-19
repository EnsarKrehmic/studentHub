namespace StudentHub.ViewModels
{
    public class PravilnikViewModel
    {
        public int Id { get; set; }
        public string Naslov { get; set; }
        public string Opis { get; set; }
        public List<PravilnikClanakViewModel> Clanovi { get; set; } = new();
    }
    public class PravilnikClanakViewModel
    {
        public int? Id { get; set; }
        public string NaslovClanka { get; set; }
        public string Sadrzaj { get; set; }
        public int RedniBroj { get; set; }
    }
}
