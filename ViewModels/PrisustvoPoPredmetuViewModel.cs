using StudentHub.Models;
using System.Collections.Generic;

namespace StudentHub.ViewModels
{
    public class PrisustvoPoPredmetuViewModel
    {
        public Predmet Predmet { get; set; }
        public List<NastavnaAktivnost> Aktivnosti { get; set; } = new();
        public List<Student> Studenti { get; set; } = new();

        // Prikaz osnovnih podataka za zaglavlje i filtre
        public string NazivPredmeta => Predmet?.Naziv ?? "";
        public string StudijskiProgram => Predmet?.StudijskiProgram?.Naziv ?? "";
        public int GodinaStudija => Predmet?.GodinaStudija ?? 0;
        public int Semestar => Predmet?.Semestar ?? 0;
        public string TipPredmetaLabel => Predmet?.TipPredmeta == TipPredmeta.Osnovni ? "Osnovni" : "Izborni";

        // Prisustvo: mapa (studentId, aktivnostId) => status ("Prisutan", "Odsutan" itd.)
        public Dictionary<(long studentId, long aktivnostId), string> StatusiPrisustva { get; set; } = new();

        // Ocjena: mapa (studentId, aktivnostId) => Ocjena (ako postoji)
        public Dictionary<(long studentId, long aktivnostId), Ocjena?> OcjeneAktivnosti { get; set; } = new();

        // Ocjene po predmetu (Tip = Predmet i ParentId == null)
        public Dictionary<long, Ocjena?> OcjenePredmeta { get; set; } = new();

        // Djelimične ocjene po predmetnoj ocjeni
        public Dictionary<long, List<Ocjena>> DjelimicneOcjenePoOcjeniId { get; set; } = new();

        // Ponderisana vrijednost zaključne ocjene (ako ima parcijalnih ocjena)
        public Dictionary<long, float?> PonderisaneZakljucneOcjene { get; set; } = new();

        // Profesor koji je unio ocjenu: (studentId, aktivnostId) => string (ime i prezime)
        public Dictionary<(long studentId, long aktivnostId), string> ProfesorOcjenjivac { get; set; } = new();

        // Bodovi sa ispita: (studentId, ispitId) => broj bodova
        public Dictionary<(long studentId, long ispitId), decimal> BodoviSaIspita { get; set; } = new();

        // Statistike prisustva po tipu aktivnosti
        public List<StudentPrisustvoStatistika> StatistikaUkupno { get; set; } = new();
        public List<StudentPrisustvoStatistika> StatistikaPredavanja { get; set; } = new();
        public List<StudentPrisustvoStatistika> StatistikaVjezbe { get; set; } = new();

        // Pragovi za automatsko ocjenjivanje ili validaciju
        public int PragPrisustvaPredavanja { get; set; }
        public int PragPrisustvaVjezbe { get; set; }
        public int PragPrisustvaUkupno { get; set; }
    }

    public class StudentPrisustvoStatistika
    {
        public Student Student { get; set; }
        public int BrojPrisustava { get; set; }
        public int UkupnoAktivnosti { get; set; }
        public float Procenat => UkupnoAktivnosti == 0 ? 0 : (float)BrojPrisustava / UkupnoAktivnosti * 100;
    }
}