using StudentHub.Models;
using X.PagedList;

namespace StudentHub.ViewModels
{
    public class StudentNaPredmetuViewModel
    {
        public Student Student { get; set; }
        public Predmet Predmet { get; set; }

        public List<NastavnaAktivnost> Aktivnosti { get; set; } = new();
        public List<PrisustvoNaAktivnosti> Prisustva { get; set; } = new();
        public List<Ocjena> Ocjene { get; set; } = new();

        public float ProcenatUkupno { get; set; }
        public float ProcenatPredavanja { get; set; }
        public float ProcenatVjezbi { get; set; }

        public float? ZakljucnaOcjena { get; set; }
        public bool DozvoljenPristup { get; set; }
    }
}