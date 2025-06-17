using StudentHub.Models;

namespace StudentHub.ViewModels
{
    public class PrisustvoPoPredmetuViewModel
    {
        public Predmet Predmet { get; set; }
        public List<NastavnaAktivnost> Aktivnosti { get; set; }
        public List<Student> Studenti { get; set; }

        // statusi po studentu i aktivnosti
        public Dictionary<(long studentId, long aktivnostId), string> StatusiPrisustva { get; set; } = new();

        // statistika prisustva po tipu aktivnosti
        public List<StudentPrisustvoStatistika> StatistikaUkupno { get; set; } = new();
        public List<StudentPrisustvoStatistika> StatistikaPredavanja { get; set; } = new();
        public List<StudentPrisustvoStatistika> StatistikaVjezbe { get; set; } = new();

        // pragovi za evaluaciju
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
