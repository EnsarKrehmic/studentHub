using StudentHub.Models;

namespace StudentHub.ViewModels
{
    public class HomeViewModel
    {
        public List<Obavjestenje> NajnovijeObavijesti { get; set; } = new();
        public int BrojAsistenata { get; set; }
        public int BrojProfesora { get; set; }
        public int BrojStudenata { get; set; }
        public int AktivniIspiti { get; set; }
        public HomeViewModel() { }
    }
}
