namespace StudentHub.ViewModels
{
    public class OcjenaViewModel
    {
        public long Id { get; set; }
        public string Tip { get; set; }

        public string PredmetNaziv { get; set; }
        public string NastavnaAktivnostNaziv { get; set; }

        public string StudentIme { get; set; }
        public string StudentPrezime { get; set; }
        public string StudentBrojIndeksa { get; set; }

        public string ProfesorIme { get; set; }
        public string ProfesorPrezime { get; set; }
        public string ProfesorTitula { get; set; }

        public float Vrijednost { get; set; }

        public DateTime? DatumDodjele { get; set; }
        public string? Komentar { get; set; }
        public int? TezinaProcentualno { get; set; }
        public long? NastavnaAktivnostId { get; set; }

        public long? ParentOcjenaId { get; set; }
        public string ParentNaziv { get; set; }
        public OcjenaViewModel? ParentOcjena { get; set; }

        public List<OcjenaViewModel> DjelimicneOcjene { get; set; } = new();
        public List<(long IspitId, string IspitNaziv, decimal Bodovi, DateTime Datum)> BodoviSaIspita { get; set; } = new();

        public double? PonderisanaVrijednost { get; set; }
        public double ProsjekOcjena { get; set; }
        public double ProsjekPoPredmetu { get; set; }
        public double ProsjekPoStudijskomProgramu { get; set; }
        public string ProsjekPrikaz { get; set; }
        public string? TipOcjeneNaziv => Tip == "Predmet" ? "Zaključna" : "Aktivnost";

        public float ProcenatPrisustvaUkupno { get; set; }
        public float ProcenatPrisustvaPredavanja { get; set; }
        public float ProcenatPrisustvaVjezbi { get; set; }

        public string StudentStudijskiProgramNaziv { get; set; }
        public long PredmetId { get; set; }
        public long StudentId { get; set; }
        public long StudijskiProgramId { get; set; }
        public long? ProfesorId { get; set; }
    }
}
