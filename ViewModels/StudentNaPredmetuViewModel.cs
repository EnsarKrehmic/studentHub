using StudentHub.Models;
using System.Collections.Generic;

namespace StudentHub.ViewModels
{
    public class StudentNaPredmetuViewModel
    {
        public Student Student { get; set; }
        public Predmet Predmet { get; set; }

        public List<NastavnaAktivnost> Aktivnosti { get; set; } = new();
        public List<PrisustvoNaAktivnosti> Prisustva { get; set; } = new();

        // Ocjene - uključuju i glavne i parcijalne ako su učitane
        public List<Ocjena> Ocjene { get; set; } = new();

        // Zaključna ocjena se posebno izračunava ili ručno dodjeljuje
        public float? ZakljucnaOcjena { get; set; }
        public float? PonderisanaOcjena { get; set; }

        // Prisustvo kao procenat ukupno, po tipu aktivnosti
        public float ProcenatUkupno { get; set; }
        public float ProcenatPredavanja { get; set; }
        public float ProcenatVjezbi { get; set; }

        // Pravo pristupa (korisno za studentski prikaz)
        public bool DozvoljenPristup { get; set; }

        // Parcijalne ocjene grupisane po ParentId
        public Dictionary<long, List<Ocjena>> ParcijalneOcjene { get; set; } = new();

        // Mapirano prisustvo po aktivnosti
        public Dictionary<long, bool> PrisustvoPoAktivnosti { get; set; } = new();

        // Ocjena po aktivnosti (ključ: nastavna aktivnost ID)
        public Dictionary<long, Ocjena?> OcjenaPoAktivnosti { get; set; } = new();

        // Ime i prezime nastavnika koji je dao ocjenu (po aktivnostima)
        public Dictionary<long, string> ProfesorPoAktivnosti { get; set; } = new();

        public List<Prijava> Prijave { get; set; } = new();

        public decimal UslovZaPolaganje { get; set; }
    }
}
