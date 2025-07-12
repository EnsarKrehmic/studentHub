using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentHub.Models
{
    public enum TipPredmeta
    {
        Osnovni = 1,
        Izborni = 2
    }

    public class Predmet
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Naziv predmeta je obavezan.")]
        [MaxLength(100, ErrorMessage = "Naziv ne može biti duži od 100 karaktera.")]
        public string Naziv { get; set; }

        [MaxLength(500, ErrorMessage = "Opis ne može biti duži od 500 karaktera.")]
        public string Opis { get; set; }

        [Required(ErrorMessage = "ECTS bodovi su obavezni.")]
        [Range(1, 30, ErrorMessage = "Broj ECTS bodova mora biti između 1 i 30.")]
        public int ECTS { get; set; }

        [Required(ErrorMessage = "Tip predmeta je obavezan.")]
        public TipPredmeta TipPredmeta { get; set; }

        [Required, Range(1, 2, ErrorMessage = "Semestar može biti samo 1 (zimski) ili 2 (ljetni).")]
        public int Semestar { get; set; }

        [Required, Range(1, 6, ErrorMessage = "Godina studija mora biti između 1 i 6.")]
        public int GodinaStudija { get; set; }

        [Required, Range(0, 60)]
        public int SatiPredavanja { get; set; }
        [Required, Range(0, 60)]
        public int SatiVjezbi { get; set; }

        [Range(0, 100)]
        public int? PragPrisustvaPredavanja { get; set; } = 70;

        [Range(0, 100)]
        public int? PragPrisustvaVjezbe { get; set; } = 70;

        [Range(0, 100)]
        public int? PragPrisustvaUkupno { get; set; } = 70;

        [ForeignKey("Profesor")]
        public long? ProfesorId { get; set; }
        public Profesor Profesor { get; set; }

        [ForeignKey("Asistent")]
        public long? AsistentId { get; set; }
        public Asistent Asistent { get; set; }

        [Required]
        [ForeignKey("StudijskiProgram")]
        public long StudijskiProgramId { get; set; }
        public StudijskiProgram StudijskiProgram { get; set; }

        [ForeignKey("NastavniPlan")]
        public long? NastavniPlanId { get; set; }
        public NastavniPlan NastavniPlan { get; set; }

        public List<StudentNaPredmetu> StudentNaPredmetima { get; set; } = new();
        public List<PredmetProfesor> PredmetProfesori { get; set; } = new();
        public List<PredmetAsistent> PredmetAsistenti { get; set; } = new();
        public List<NastavnaAktivnost> NastavneAktivnosti { get; set; } = new();

        public Predmet() { }
    }
}
