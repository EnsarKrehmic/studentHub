using StudentHub.Models;

namespace StudentHub.ViewModels
{
    public class KorisnikProfileViewModel
    {
        public long Id { get; set; }
        public string JMBG { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string? Email { get; set; }
        public Uloga Uloga { get; set; }
        public Student? Student { get; set; }
        public Profesor? Profesor { get; set; }
        public Asistent? Asistent { get; set; }
        public StudentskaSluzba? StudentskaSluzba { get; set; }
        public List<Dokument> Dokumenti { get; set; }
        public List<Zahtjev> Zahtjevi { get; set; }
        public List<Uvjerenje> Uvjerenja { get; set; }
    }
}