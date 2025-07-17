using System;
using System.Collections.Generic;

namespace StudentHub.ViewModels
{
    public class MojiPodaciIndexViewModel
    {
        // --- Zajedničko za sve korisnike ---
        public string Uloga { get; set; }
        public long KorisnikId { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string? Email { get; set; }
        public string? ProfilnaSlikaUrl { get; set; }
        public string QrKodBase64 { get; set; }

        // --- Student specifično ---
        public string? BrojIndeksa { get; set; }
        public int? GodinaStudija { get; set; }
        public int? Semestar { get; set; }
        public string? StudijskiProgram { get; set; }
        public bool IzborIzbornihPredmetaZakljucan { get; set; }
        public double ProsjekOcjena { get; set; }
        public int BrojPolozenihIspita { get; set; }
        public int BrojNepolozenihPredmeta { get; set; }
        public int BrojPrijavljenihIspita { get; set; }
        public int BrojUkupnoPrisustava { get; set; }
        public double ProcenatPrisustva { get; set; }
        public List<PredmetInfoVM> UpisaniPredmeti { get; set; } = new();
        public List<IspitInfoVM> NadolazeciIspiti { get; set; } = new();
        public List<KorisnikAktivnostVM> TimelineAktivnosti { get; set; } = new();
        public List<KontaktPredmetVM> BrziKontakti { get; set; } = new();

        // Vizualizacija statistike (grafikoni)
        public List<int> PolozenoPoSemestru { get; set; } = new();
        public List<string> SemestriLabels { get; set; } = new();
        public int BrojPolozenihObaveznih { get; set; }
        public int BrojPolozenihIzbornih { get; set; }

        // To-do/Preporučene akcije
        public List<string> TodoAkcije { get; set; } = new();

        // Mini-kalendar / naredni termin
        public SljedeciTerminVM SljedeciTermin { get; set; }

        // Vizualni prikaz prisustva
        public int PrisustvoBarValue { get; set; }

        // --- Profesor / Asistent specifično ---
        public string? Titula { get; set; }
        public List<PredmetInfoVM> PredmetiPredaje { get; set; } = new();
        public int BrojZahtjevaZaPrisustvo { get; set; }
        public List<IspitInfoVM> NadolazeciIspitiZaPredmete { get; set; } = new();
        public List<StudentStatistikaVM> StatistikaPoPredmetu { get; set; } = new();
        public List<string> TodoAkcijeNastavnik { get; set; } = new();

        // Za grafikone – npr. prosjek po predmetima
        public List<string> PredmetiLabels { get; set; } = new();
        public List<double> ProsjekPoPredmetu { get; set; } = new();

        // --- Studentska služba specifično ---
        public int BrojNerijesenihZahtjeva { get; set; }
        public int BrojUpisanihStudenata { get; set; }
        public int BrojUpisanihProfesora { get; set; }
        public int BrojUpisanihAsistenata { get; set; }
        public int BrojOtvorenihUpita { get; set; }
        public int BrojZatvorenihUpita { get; set; }
        public int BrojOtvorenihBugova { get; set; }
        public int BrojZatvorenihBugova { get; set; }
        public List<StatistikaProgramVM> StatistikaPoProgramima { get; set; } = new();
        public List<string> TodoAkcijeSluzba { get; set; } = new();

        // Za grafikone – generacije
        public List<string> GeneracijeLabels { get; set; } = new();
        public List<int> BrojUpisanihPoGeneraciji { get; set; } = new();

        // --- Sistemske notifikacije / poruke ---
        public List<SystemNotifikacijaVM> ZadnjeNotifikacije { get; set; } = new();

        // --- NOVO: Podrška i prijave ---
        public int BrojMojihUpita { get; set; }
        public int BrojMojihBugova { get; set; }
    }

    // --- Brzi kontakti: SVI profesori i asistenti po predmetu ---
    public class KontaktPredmetVM
    {
        public string PredmetNaziv { get; set; }
        public List<KontaktOsobaVM> Profesori { get; set; } = new();
        public List<KontaktOsobaVM> Asistenti { get; set; } = new();
        public string SluzbaIme { get; set; }
        public string SluzbaEmail { get; set; }
    }

    public class KontaktOsobaVM
    {
        public string ImePrezime { get; set; }
        public string Email { get; set; }
        public bool Glavni { get; set; }
    }
    public class PredmetInfoVM
    {
        public long Id { get; set; }
        public string Naziv { get; set; }
        public string? Profesor { get; set; }
        public string? Asistent { get; set; }
        public int ECTS { get; set; }
        public int Semestar { get; set; }
        public string TipPredmeta { get; set; }
        public double? ProsjecnaOcjena { get; set; }
        public int? BrojStudenata { get; set; }
    }

    public class IspitInfoVM
    {
        public long Id { get; set; }
        public string NazivPredmeta { get; set; }
        public DateTime Datum { get; set; }
        public bool MozeSePrijaviti { get; set; }
        public bool Prijavljen { get; set; }
        public int? Ocjena { get; set; }
    }

    public class KorisnikAktivnostVM
    {
        public DateTime Vrijeme { get; set; }
        public string Opis { get; set; }
        public string TipAktivnosti { get; set; }
    }

    public class StudentStatistikaVM
    {
        public string NazivPredmeta { get; set; }
        public double ProsjecnaOcjena { get; set; }
        public int BrojStudenata { get; set; }
    }

    public class StatistikaProgramVM
    {
        public string NazivPrograma { get; set; }
        public int BrojStudenata { get; set; }
        public double ProsjekOcjena { get; set; }
    }

    public class SljedeciTerminVM
    {
        public DateTime Datum { get; set; }
        public string Predmet { get; set; }
        public string Lokacija { get; set; }
        public string TipAktivnosti { get; set; }
    }

    public class SystemNotifikacijaVM
    {
        public DateTime Vrijeme { get; set; }
        public string Sadrzaj { get; set; }
        public string Tip { get; set; }
    }
}
