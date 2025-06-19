using System.Collections.Generic;
using StudentHub.Models;

namespace StudentHub.ViewModels
{
    public class PredmetDetailsViewModel
    {
        public Predmet Predmet { get; set; }

        public List<PredmetProfesor> Profesori { get; set; } = new();
        public List<PredmetAsistent> Asistenti { get; set; } = new();
        public List<StudentNaPredmetu> StudentiNaPredmetu { get; set; } = new();
        public List<NastavnaAktivnost> NastavneAktivnosti { get; set; } = new();

        // Mapa: StudentId -> Ocjena (vrijednost)
        public Dictionary<long, float?> Ocjene { get; set; } = new();

        // Mapa: StudentId -> Ocjena.Id (koristi se za linkovanje ka Details/Edit/Delete)
        public Dictionary<long, long> OcjenaIds { get; set; } = new();

        // Statistika prisustva za studente
        public List<StatistikaPrisustvaDTO> StatistikaPrisustva { get; set; } = new();
        public float? ProsjecnoPrisustvo { get; set; }
        public float? ProsjecnaOcjena { get; set; }

        // Pomoćna polja za forme
        public long StudentId { get; set; }
        public long ProfesorId { get; set; }
        public long AsistentId { get; set; }

        public int UkupnoStudenata { get; set; }
        public int BrojPoloziliPredmet { get; set; }
    }

    public class StatistikaPrisustvaDTO
    {
        public Student Student { get; set; }
        public int BrojPrisustava { get; set; }
        public int UkupnoAktivnosti { get; set; }

        public float Procenat => UkupnoAktivnosti == 0 ? 0 : (float)BrojPrisustava / UkupnoAktivnosti * 100;
    }
}
