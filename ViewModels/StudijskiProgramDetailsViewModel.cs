using StudentHub.Models;

namespace StudentHub.ViewModels
{
    public class ObavjestenjeDetailsViewModel
    {
        public long Id { get; set; }
        public string Naslov { get; set; }
        public string Sadrzaj { get; set; }
        public DateTime DatumObjave { get; set; }
        public string AutorIme { get; set; }
    }

    public class StudijskiProgramDetailsViewModel
    {
        public StudijskiProgram StudijskiProgram { get; set; }
        public List<ObavjestenjeDetailsViewModel> Obavjestenja { get; set; }
        public int BrojStudenata { get; set; }
        public int BrojProfesora { get; set; }
        public int BrojAsistenata { get; set; }
    }
}
