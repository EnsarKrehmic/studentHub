using StudentHub.Models;

namespace StudentHub.ViewModels
{
    public class ObavjestenjeViewModel
    {
        public long Id { get; set; }
        public string Naslov { get; set; }
        public string Sadrzaj { get; set; }
        public DateTime DatumObjave { get; set; }
        public string AutorIme { get; set; }
        public string KorisnikAspNetUserId { get; set; }
        public string AutorAspNetUserId { get; set; }
        public List<string> StudijskiProgramNazivi { get; set; } = new List<string>();
    }
}