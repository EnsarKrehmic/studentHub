using StudentHub.Models;
using System.Collections.Generic;

namespace StudentHub.ViewModels
{
    public class PrisustvoPoPredmetuViewModel
    {
        public Predmet Predmet { get; set; }
        public List<NastavnaAktivnost> Aktivnosti { get; set; } = new();
        public List<Student> Studenti { get; set; } = new();

        // Osnovna svojstva za prikaz zaglavlja i informacije
        public string NazivPredmeta => Predmet?.Naziv ?? "";
        public string StudijskiProgram => Predmet?.StudijskiProgram?.Naziv ?? "";
        public int GodinaStudija => Predmet?.GodinaStudija ?? 0;
        public int Semestar => Predmet?.Semestar ?? 0;
        public string TipPredmetaLabel => Predmet?.TipPredmeta == TipPredmeta.Osnovni ? "Osnovni" : "Izborni";
        public decimal UkupnoBodova => Predmet?.UkupnoBodova ?? 0; // NOVO: eksplicitno ako želiš

        public Dictionary<(long studentId, long aktivnostId), string> StatusiPrisustva { get; set; } = new();
        public Dictionary<(long studentId, long aktivnostId), Ocjena?> OcjeneAktivnosti { get; set; } = new();
        public Dictionary<long, Ocjena?> OcjenePredmeta { get; set; } = new();
        public Dictionary<long, List<Ocjena>> DjelimicneOcjenePoOcjeniId { get; set; } = new();
        public Dictionary<long, float?> PonderisaneZakljucneOcjene { get; set; } = new();
        public Dictionary<(long studentId, long aktivnostId), string> ProfesorOcjenjivac { get; set; } = new();
        public Dictionary<(long studentId, long ispitId), decimal> BodoviSaIspita { get; set; } = new();

        public List<StudentPrisustvoStatistika> StatistikaUkupno { get; set; } = new();
        public List<StudentPrisustvoStatistika> StatistikaPredavanja { get; set; } = new();
        public List<StudentPrisustvoStatistika> StatistikaVjezbe { get; set; } = new();

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
