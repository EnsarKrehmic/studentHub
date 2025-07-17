using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace StudentHub.Controllers
{
    [Authorize]
    [Route("MojiPodaci")]
    public class MojiPodaciController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MojiPodaciController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public MojiPodaciController(ApplicationDbContext context, ILogger<MojiPodaciController> logger, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var aspNetUserId = _userManager.GetUserId(User);

            // Dohvati osnovne podatke o korisniku (bilo kojoj ulozi)
            var korisnik = await _context.Korisnici.FirstOrDefaultAsync(k => k.AspNetUserId == aspNetUserId);
            if (korisnik == null)
                return Forbid();

            var model = new MojiPodaciIndexViewModel
            {
                Uloga = korisnik.Uloga.ToString(),
                KorisnikId = korisnik.Id,
                Ime = korisnik.Ime,
                Prezime = korisnik.Prezime,
                Email = korisnik.Email,
                QrKodBase64 = GenerisiQrKodBase64(korisnik)
            };

            // Osnovni podaci po ulogama
            switch (korisnik.Uloga)
            {
                // === STUDENT ===
                case Uloga.Student:
                    {
                        var student = await _context.Studenti
                            .Include(s => s.StudentNaPredmetima).ThenInclude(snp => snp.Predmet)
                            .Include(s => s.StudentStudijskiProgrami).ThenInclude(sp => sp.StudijskiProgram)
                            .FirstOrDefaultAsync(s => s.Id == korisnik.Id);

                        if (student == null) break;

                        model.BrojIndeksa = student.BrojIndeksa;
                        model.GodinaStudija = student.GodinaStudija;
                        model.Semestar = student.Semestar;
                        model.IzborIzbornihPredmetaZakljucan = student.IzborIzbornihPredmetaZakljucan;
                        model.StudijskiProgram = student.StudentStudijskiProgrami.FirstOrDefault()?.StudijskiProgram?.Naziv;

                        // === Personalizacija: Profilna slika ===
                        model.ProfilnaSlikaUrl = $"/images/profili/{student.Id}.jpg"; // ili null/placeholder ako nema slike

                        // --- Statistika ocjena ---
                        var ocjene = await _context.Ocjene
                            .Where(o => o.StudentId == student.Id && o.Vrijednost >= 6)
                            .ToListAsync();
                        model.BrojPolozenihIspita = ocjene.Count;
                        model.ProsjekOcjena = ocjene.Count > 0 ? Math.Round(ocjene.Average(o => o.Vrijednost), 2) : 0;

                        // --- Predmeti ---
                        model.UpisaniPredmeti = student.StudentNaPredmetima.Select(snp => new PredmetInfoVM
                        {
                            Id = snp.Predmet.Id,
                            Naziv = snp.Predmet.Naziv,
                            Profesor = _context.Profesori.FirstOrDefault(p => p.Id == snp.Predmet.ProfesorId)?.Ime + " " +
                                       _context.Profesori.FirstOrDefault(p => p.Id == snp.Predmet.ProfesorId)?.Prezime,
                            Asistent = _context.Asistenti.FirstOrDefault(a => a.Id == snp.Predmet.AsistentId)?.Ime + " " +
                                       _context.Asistenti.FirstOrDefault(a => a.Id == snp.Predmet.AsistentId)?.Prezime,
                            ECTS = snp.Predmet.ECTS,
                            Semestar = snp.Predmet.Semestar,
                            TipPredmeta = snp.Predmet.TipPredmeta.ToString()
                        }).ToList();

                        var predmetiIds = student.StudentNaPredmetima.Select(snp => snp.PredmetId).ToList();

                        // --- Nepoloženi predmeti ---
                        var polozeniPredmetiIds = await _context.Ocjene
                            .Where(o => o.StudentId == student.Id
                                && o.Vrijednost >= 6
                                && o.PredmetId.HasValue
                                && predmetiIds.Contains(o.PredmetId.Value))
                            .Select(o => o.PredmetId.Value)
                            .Distinct()
                            .ToListAsync();

                        model.BrojNepolozenihPredmeta = predmetiIds.Count(pid => !polozeniPredmetiIds.Contains(pid));

                        // --- Prisustvo ---
                        var ukupnoPrisustava = await _context.PrisustvaNaAktivnostima.CountAsync(p => p.StudentId == student.Id);
                        model.BrojUkupnoPrisustava = ukupnoPrisustava;
                        var sveAktivnosti = await _context.NastavneAktivnosti
                            .Where(na => predmetiIds.Contains(na.PredmetId))
                            .CountAsync();
                        model.ProcenatPrisustva = sveAktivnosti > 0 ? Math.Round((ukupnoPrisustava * 100.0) / sveAktivnosti, 1) : 0;
                        model.PrisustvoBarValue = (int)model.ProcenatPrisustva;

                        // --- Vizualizacija: položeni obavezni/izborni i po semestrima ---
                        model.BrojPolozenihObaveznih = ocjene
                            .Count(o => student.StudentNaPredmetima.Any(snp => snp.PredmetId == o.PredmetId && snp.Predmet.TipPredmeta == TipPredmeta.Osnovni));
                        model.BrojPolozenihIzbornih = ocjene
                            .Count(o => student.StudentNaPredmetima.Any(snp => snp.PredmetId == o.PredmetId && snp.Predmet.TipPredmeta == TipPredmeta.Izborni));

                        // Položeni po semestrima:
                        var semestri = student.StudentNaPredmetima
                            .Select(snp => snp.Predmet.Semestar)
                            .Distinct()
                            .OrderBy(x => x)
                            .ToList();

                        model.PolozenoPoSemestru = semestri.Select(sem =>
                            ocjene.Count(o => student.StudentNaPredmetima.Any(snp => snp.PredmetId == o.PredmetId && snp.Predmet.Semestar == sem))
                        ).ToList();

                        model.SemestriLabels = semestri.Select(sem => $"Semestar {sem}").ToList();

                        // --- Nadolazeći ispiti ---
                        var danas = DateTime.Now.Date;
                        model.NadolazeciIspiti = await _context.Ispiti
                            .Include(i => i.Predmet)
                            .Where(i => i.DatumOdrzavanja >= danas && predmetiIds.Contains(i.PredmetId))
                            .OrderBy(i => i.DatumOdrzavanja)
                            .Take(5)
                            .Select(i => new IspitInfoVM
                            {
                                Id = i.Id,
                                NazivPredmeta = i.Predmet.Naziv,
                                Datum = i.DatumOdrzavanja,
                                MozeSePrijaviti = !_context.Prijave.Any(p => p.StudentId == student.Id && p.IspitId == i.Id),
                                Prijavljen = _context.Prijave.Any(p => p.StudentId == student.Id && p.IspitId == i.Id),
                                Ocjena = _context.Ocjene
                                    .Where(o => o.PredmetId == i.PredmetId && o.StudentId == student.Id)
                                    .Select(o => (int?)Math.Round(o.Vrijednost))
                                    .FirstOrDefault()
                            }).ToListAsync();

                        // --- Mini-kalendar: sljedeći termin ---
                        // Pronađi sve termine za studenta
                        var termini = await _context.TerminiNastave
                            .Include(t => t.Predmet)
                            .Where(t => t.Raspored != null && predmetiIds.Contains(t.PredmetId))
                            .ToListAsync();

                        if (termini.Any())
                        {
                            var now = DateTime.Now;
                            var danasDan = now.DayOfWeek;
                            var trenutnoVrijeme = now.TimeOfDay;

                            // Pronađi prvi naredni termin u ovoj sedmici (od danas pa dalje)
                            var naredniTermin = termini
                                .Where(t =>
                                    (t.Dan > danasDan) ||
                                    (t.Dan == danasDan && t.VrijemeOd > trenutnoVrijeme))
                                .OrderBy(t => t.Dan)
                                .ThenBy(t => t.VrijemeOd)
                                .FirstOrDefault();

                            // Ako nema više termina u ovoj sedmici, uzmi prvi u idućoj (po danu i vremenu)
                            if (naredniTermin == null)
                            {
                                naredniTermin = termini
                                    .OrderBy(t => (int)t.Dan)
                                    .ThenBy(t => t.VrijemeOd)
                                    .FirstOrDefault();
                            }

                            if (naredniTermin != null)
                            {
                                // Izračunaj tačan datum termina
                                int danasInt = (int)danasDan;
                                int terminInt = (int)naredniTermin.Dan;
                                int daniDoTermina = (terminInt - danasInt + 7) % 7;
                                var datumTermina = now.Date.AddDays(daniDoTermina);

                                model.SljedeciTermin = new SljedeciTerminVM
                                {
                                    Datum = datumTermina.Add(naredniTermin.VrijemeOd),
                                    Predmet = naredniTermin.Predmet?.Naziv,
                                    Lokacija = naredniTermin.Lokacija,
                                    TipAktivnosti = naredniTermin.Vrsta.ToString()
                                };
                            }
                        }

                        // --- Preporučene akcije / to-do ---
                        model.TodoAkcije = new List<string>();
                        if (model.NadolazeciIspiti.Any(i => !i.Prijavljen && i.MozeSePrijaviti))
                        {
                            var najbliziIspit = model.NadolazeciIspiti.Where(i => !i.Prijavljen && i.MozeSePrijaviti).OrderBy(i => i.Datum).First();
                            model.TodoAkcije.Add($"Prijavi se na ispit iz predmeta <strong>{najbliziIspit.NazivPredmeta}</strong> ({najbliziIspit.Datum:dd.MM.yyyy})");
                        }
                        if (!student.IzborIzbornihPredmetaZakljucan)
                        {
                            model.TodoAkcije.Add("Možete izabrati izborne predmete za tekuću godinu.");
                        }

                        // --- Brzi kontakti ---
                        // Prvo pronađi službu za program (prva, ili prema programu studenta)
                        var programId = student.StudentStudijskiProgrami.FirstOrDefault()?.StudijskiProgramId;
                        var sluzbaVeza = await _context.StudentskaSluzbaStudijskiProgrami
                            .Include(ssp => ssp.StudentskaSluzba)
                            .FirstOrDefaultAsync(ssp => ssp.StudijskiProgramId == programId);

                        var sluzbaIme = sluzbaVeza?.StudentskaSluzba?.Ime + " " + sluzbaVeza?.StudentskaSluzba?.Prezime;
                        var sluzbaEmail = sluzbaVeza?.StudentskaSluzba?.Email;

                        model.BrziKontakti = new List<KontaktPredmetVM>();
                        foreach (var snp in student.StudentNaPredmetima)
                        {
                            var predmet = snp.Predmet;

                            // SVI profesori
                            var profesori = await _context.PredmetProfesori
                                .Include(pp => pp.Profesor)
                                .Where(pp => pp.PredmetId == predmet.Id)
                                .ToListAsync();

                            var profesoriVM = profesori.Select(pp => new KontaktOsobaVM
                            {
                                ImePrezime = pp.Profesor.Ime + " " + pp.Profesor.Prezime,
                                Email = pp.Profesor.Email,
                                Glavni = predmet.ProfesorId == pp.ProfesorId
                            }).ToList();

                            // SVI asistenti
                            var asistenti = await _context.PredmetAsistenti
                                .Include(pa => pa.Asistent)
                                .Where(pa => pa.PredmetId == predmet.Id)
                                .ToListAsync();

                            var asistentiVM = asistenti.Select(pa => new KontaktOsobaVM
                            {
                                ImePrezime = pa.Asistent.Ime + " " + pa.Asistent.Prezime,
                                Email = pa.Asistent.Email,
                                Glavni = predmet.AsistentId == pa.AsistentId
                            }).ToList();

                            model.BrziKontakti.Add(new KontaktPredmetVM
                            {
                                PredmetNaziv = predmet.Naziv,
                                Profesori = profesoriVM,
                                Asistenti = asistentiVM,
                                SluzbaIme = sluzbaIme,
                                SluzbaEmail = sluzbaEmail
                            });
                        }

                        // --- Timeline aktivnosti (posljednjih 10) ---
                        model.TimelineAktivnosti = await _context.Prijave
                            .Where(p => p.StudentId == student.Id)
                            .OrderByDescending(p => p.DatumPrijave)
                            .Take(10)
                            .Select(p => new KorisnikAktivnostVM
                            {
                                Vrijeme = p.DatumPrijave,
                                Opis = $"Prijava na ispit ({p.Ispit.Predmet.Naziv})",
                                TipAktivnosti = "Prijava ispita"
                            }).ToListAsync();

                        // --- Podrška: broj vlastitih upita i prijava ---
                        model.BrojMojihUpita = await _context.PodrskaUpiti.CountAsync(u => u.KorisnikId == korisnik.Id);
                        model.BrojMojihBugova = await _context.BugReporti.CountAsync(u => u.KorisnikId == korisnik.Id);

                        break;
                    }
                case Uloga.Profesor:
                    {
                        var profesor = await _context.Profesori
                            .Include(p => p.Predmeti).ThenInclude(pp => pp.Predmet)
                            .FirstOrDefaultAsync(p => p.Id == korisnik.Id);

                        if (profesor == null) break;

                        model.Titula = profesor.ProfesorTitula;
                        model.ProfilnaSlikaUrl = $"/images/profili/{profesor.Id}.jpg"; // ili default slika

                        var predmeti = profesor.Predmeti.Select(pp => pp.Predmet).ToList();
                        var predmetiIds = predmeti.Select(p => p.Id).ToList();

                        // Predmeti koje predaje
                        model.PredmetiPredaje = predmeti.Select(p => new PredmetInfoVM
                        {
                            Id = p.Id,
                            Naziv = p.Naziv,
                            ECTS = p.ECTS,
                            Semestar = p.Semestar,
                            TipPredmeta = p.TipPredmeta.ToString(),
                            BrojStudenata = _context.StudentiNaPredmetima.Count(snp => snp.PredmetId == p.Id),
                            ProsjecnaOcjena = _context.Ocjene
                                .Where(o => o.PredmetId.HasValue && o.PredmetId.Value == p.Id && o.Vrijednost >= 6)
                                .Any()
                                ? Math.Round(_context.Ocjene
                                    .Where(o => o.PredmetId.HasValue && o.PredmetId.Value == p.Id && o.Vrijednost >= 6)
                                    .Average(o => o.Vrijednost), 2)
                                : (double?)null
                        }).ToList();

                        // Zahtjevi za prisustvo
                        model.BrojZahtjevaZaPrisustvo = await _context.ZahtjeviZaPrisustvo
                            .Include(z => z.NastavnaAktivnost).ThenInclude(na => na.Predmet)
                            .Where(z => !z.Obradjen && z.NastavnaAktivnost.Predmet.ProfesorId == korisnik.Id)
                            .CountAsync();

                        // Nadolazeći ispiti za predmete koje predaje
                        var danas = DateTime.Now.Date;
                        model.NadolazeciIspitiZaPredmete = await _context.Ispiti
                            .Include(i => i.Predmet)
                            .Where(i => i.DatumOdrzavanja >= danas && predmetiIds.Contains(i.PredmetId))
                            .OrderBy(i => i.DatumOdrzavanja)
                            .Take(5)
                            .Select(i => new IspitInfoVM
                            {
                                Id = i.Id,
                                NazivPredmeta = i.Predmet.Naziv,
                                Datum = i.DatumOdrzavanja,
                            }).ToListAsync();

                        // Statistika po predmetima (za bar chart)
                        model.PredmetiLabels = predmeti.Select(p => p.Naziv).ToList();
                        model.ProsjekPoPredmetu = predmeti.Select(p =>
                            _context.Ocjene.Where(o => o.PredmetId.HasValue && o.PredmetId.Value == p.Id && o.Vrijednost >= 6).Any()
                            ? Math.Round(_context.Ocjene.Where(o => o.PredmetId.HasValue && o.PredmetId.Value == p.Id && o.Vrijednost >= 6).Average(o => o.Vrijednost), 2)
                            : 0
                        ).ToList();

                        // Mini-kalendar: sljedeći termin nastave (ista logika kao za studenta)
                        var termini = await _context.TerminiNastave
                            .Include(t => t.Predmet)
                            .Where(t => t.Raspored != null && predmetiIds.Contains(t.PredmetId))
                            .ToListAsync();

                        if (termini.Any())
                        {
                            var now = DateTime.Now;
                            var danasDan = now.DayOfWeek;
                            var trenutnoVrijeme = now.TimeOfDay;

                            var naredniTermin = termini
                                .Where(t =>
                                    (t.Dan > danasDan) ||
                                    (t.Dan == danasDan && t.VrijemeOd > trenutnoVrijeme))
                                .OrderBy(t => t.Dan)
                                .ThenBy(t => t.VrijemeOd)
                                .FirstOrDefault();

                            if (naredniTermin == null)
                            {
                                naredniTermin = termini
                                    .OrderBy(t => (int)t.Dan)
                                    .ThenBy(t => t.VrijemeOd)
                                    .FirstOrDefault();
                            }

                            if (naredniTermin != null)
                            {
                                int danasInt = (int)danasDan;
                                int terminInt = (int)naredniTermin.Dan;
                                int daniDoTermina = (terminInt - danasInt + 7) % 7;
                                var datumTermina = now.Date.AddDays(daniDoTermina);

                                model.SljedeciTermin = new SljedeciTerminVM
                                {
                                    Datum = datumTermina.Add(naredniTermin.VrijemeOd),
                                    Predmet = naredniTermin.Predmet?.Naziv,
                                    Lokacija = naredniTermin.Lokacija,
                                    TipAktivnosti = naredniTermin.Vrsta.ToString()
                                };
                            }
                        }

                        // To-do / preporučene akcije (npr. zahtjevi, ispiti, poruke)
                        model.TodoAkcijeNastavnik = new List<string>();
                        if (model.BrojZahtjevaZaPrisustvo > 0)
                            model.TodoAkcijeNastavnik.Add($"Pregledajte <b>{model.BrojZahtjevaZaPrisustvo}</b> novih zahtjeva za priznavanje prisustva.");
                        if (model.NadolazeciIspitiZaPredmete.Any())
                            model.TodoAkcijeNastavnik.Add($"Pripremite se za naredne ispite na svojim predmetima.");

                        // --- Broj upita i bug prijava za asistenta ---
                        model.BrojMojihUpita = await _context.PodrskaUpiti.CountAsync(u => u.KorisnikId == korisnik.Id);
                        model.BrojMojihBugova = await _context.BugReporti.CountAsync(u => u.KorisnikId == korisnik.Id);

                        break;
                    }
                case Uloga.Asistent:
                    {
                        var asistent = await _context.Asistenti
                            .Include(a => a.Predmeti).ThenInclude(ap => ap.Predmet)
                            .FirstOrDefaultAsync(a => a.Id == korisnik.Id);

                        if (asistent == null) break;

                        model.Titula = asistent.AsistentTitula;
                        model.ProfilnaSlikaUrl = $"/images/profili/{asistent.Id}.jpg";

                        var predmeti = asistent.Predmeti.Select(ap => ap.Predmet).ToList();
                        var predmetiIds = predmeti.Select(p => p.Id).ToList();

                        // Predmeti na kojima asistira
                        model.PredmetiPredaje = predmeti.Select(p => new PredmetInfoVM
                        {
                            Id = p.Id,
                            Naziv = p.Naziv,
                            ECTS = p.ECTS,
                            Semestar = p.Semestar,
                            TipPredmeta = p.TipPredmeta.ToString(),
                            BrojStudenata = _context.StudentiNaPredmetima.Count(snp => snp.PredmetId == p.Id),
                            ProsjecnaOcjena = _context.Ocjene
                                .Where(o => o.PredmetId.HasValue && o.PredmetId.Value == p.Id && o.Vrijednost >= 6)
                                .Any()
                                ? Math.Round(_context.Ocjene
                                    .Where(o => o.PredmetId.HasValue && o.PredmetId.Value == p.Id && o.Vrijednost >= 6)
                                    .Average(o => o.Vrijednost), 2)
                                : (double?)null
                        }).ToList();

                        // Zahtjevi za prisustvo
                        model.BrojZahtjevaZaPrisustvo = await _context.ZahtjeviZaPrisustvo
                            .Include(z => z.NastavnaAktivnost).ThenInclude(na => na.Predmet)
                            .Where(z => !z.Obradjen && z.NastavnaAktivnost.Predmet.AsistentId == korisnik.Id)
                            .CountAsync();

                        // Nadolazeći ispiti za predmete na kojima asistira
                        var danas = DateTime.Now.Date;
                        model.NadolazeciIspitiZaPredmete = await _context.Ispiti
                            .Include(i => i.Predmet)
                            .Where(i => i.DatumOdrzavanja >= danas && predmetiIds.Contains(i.PredmetId))
                            .OrderBy(i => i.DatumOdrzavanja)
                            .Take(5)
                            .Select(i => new IspitInfoVM
                            {
                                Id = i.Id,
                                NazivPredmeta = i.Predmet.Naziv,
                                Datum = i.DatumOdrzavanja,
                            }).ToListAsync();

                        // Statistika po predmetima (za bar chart)
                        model.PredmetiLabels = predmeti.Select(p => p.Naziv).ToList();
                        model.ProsjekPoPredmetu = predmeti.Select(p =>
                            _context.Ocjene.Where(o => o.PredmetId.HasValue && o.PredmetId.Value == p.Id && o.Vrijednost >= 6).Any()
                            ? Math.Round(_context.Ocjene.Where(o => o.PredmetId.HasValue && o.PredmetId.Value == p.Id && o.Vrijednost >= 6).Average(o => o.Vrijednost), 2)
                            : 0
                        ).ToList();

                        // Mini-kalendar: sljedeći termin nastave (ista logika kao za studenta)
                        var termini = await _context.TerminiNastave
                            .Include(t => t.Predmet)
                            .Where(t => t.Raspored != null && predmetiIds.Contains(t.PredmetId))
                            .ToListAsync();

                        if (termini.Any())
                        {
                            var now = DateTime.Now;
                            var danasDan = now.DayOfWeek;
                            var trenutnoVrijeme = now.TimeOfDay;

                            var naredniTermin = termini
                                .Where(t =>
                                    (t.Dan > danasDan) ||
                                    (t.Dan == danasDan && t.VrijemeOd > trenutnoVrijeme))
                                .OrderBy(t => t.Dan)
                                .ThenBy(t => t.VrijemeOd)
                                .FirstOrDefault();

                            if (naredniTermin == null)
                            {
                                naredniTermin = termini
                                    .OrderBy(t => (int)t.Dan)
                                    .ThenBy(t => t.VrijemeOd)
                                    .FirstOrDefault();
                            }

                            if (naredniTermin != null)
                            {
                                int danasInt = (int)danasDan;
                                int terminInt = (int)naredniTermin.Dan;
                                int daniDoTermina = (terminInt - danasInt + 7) % 7;
                                var datumTermina = now.Date.AddDays(daniDoTermina);

                                model.SljedeciTermin = new SljedeciTerminVM
                                {
                                    Datum = datumTermina.Add(naredniTermin.VrijemeOd),
                                    Predmet = naredniTermin.Predmet?.Naziv,
                                    Lokacija = naredniTermin.Lokacija,
                                    TipAktivnosti = naredniTermin.Vrsta.ToString()
                                };
                            }
                        }

                        // To-do / preporučene akcije (npr. zahtjevi, ispiti, poruke)
                        model.TodoAkcijeNastavnik = new List<string>();
                        if (model.BrojZahtjevaZaPrisustvo > 0)
                            model.TodoAkcijeNastavnik.Add($"Pregledajte <b>{model.BrojZahtjevaZaPrisustvo}</b> novih zahtjeva za priznavanje prisustva.");
                        if (model.NadolazeciIspitiZaPredmete.Any())
                            model.TodoAkcijeNastavnik.Add($"Pripremite se za naredne ispite na predmetima gdje asistirate.");

                        // --- Broj upita i bug prijava za asistenta ---
                        model.BrojMojihUpita = await _context.PodrskaUpiti.CountAsync(u => u.KorisnikId == korisnik.Id);
                        model.BrojMojihBugova = await _context.BugReporti.CountAsync(u => u.KorisnikId == korisnik.Id);

                        break;
                    }
                case Uloga.StudentskaSluzba:
                    {
                        model.ProfilnaSlikaUrl = $"/images/profili/{korisnik.Id}.jpg";

                        model.BrojNerijesenihZahtjeva = await _context.Zahtjevi.CountAsync(z => z.StatusZahtjeva == StatusZahtjeva.Podnešen);

                        model.BrojUpisanihStudenata = await _context.Studenti.CountAsync();
                        model.BrojUpisanihProfesora = await _context.Profesori.CountAsync();
                        model.BrojUpisanihAsistenata = await _context.Asistenti.CountAsync();

                        // Statistika po programima
                        var programi = await _context.StudijskiProgrami.ToListAsync();
                        var sviStudentiPoProgramu = await _context.StudentStudijskiProgrami
                            .GroupBy(ssp => ssp.StudijskiProgramId)
                            .ToDictionaryAsync(g => g.Key, g => g.Select(ssp => ssp.StudentId).ToList());

                        var ocjene = await _context.Ocjene
                            .Where(o => o.Vrijednost >= 6)
                            .ToListAsync();

                        model.StatistikaPoProgramima = programi.Select(sp =>
                        {
                            var studentiIds = sviStudentiPoProgramu.ContainsKey(sp.Id)
                                ? sviStudentiPoProgramu[sp.Id]
                                : new List<long>();

                            var ocjeneZaProgram = ocjene
                                .Where(o => studentiIds.Contains(o.StudentId))
                                .ToList();

                            double prosjek = ocjeneZaProgram.Any()
                                ? Math.Round(ocjeneZaProgram.Average(o => o.Vrijednost), 2)
                                : 0;

                            return new StatistikaProgramVM
                            {
                                NazivPrograma = sp.Naziv,
                                BrojStudenata = studentiIds.Count,
                                ProsjekOcjena = prosjek
                            };
                        }).ToList();

                        // Vizualizacija: Broj studenata po godini studija
                        var studentiPoGodini = await _context.Studenti
                            .GroupBy(s => s.GodinaStudija)
                            .Select(g => new { Godina = g.Key, Broj = g.Count() })
                            .OrderBy(g => g.Godina)
                            .ToListAsync();

                        model.GeneracijeLabels = studentiPoGodini
                            .Where(g => g.Godina.HasValue)
                            .Select(g => $"Godina {g.Godina}").ToList();

                        model.BrojUpisanihPoGeneraciji = studentiPoGodini
                            .Where(g => g.Godina.HasValue)
                            .Select(g => g.Broj).ToList();

                        // Svi upiti i bug prijave (studentska služba vidi sve)
                        model.BrojMojihUpita = await _context.PodrskaUpiti.CountAsync();
                        model.BrojMojihBugova = await _context.BugReporti.CountAsync();

                        // Broj otvorenih i zatvorenih upita/prijava
                        model.BrojOtvorenihUpita = await _context.PodrskaUpiti
                            .CountAsync(u => u.Status == UpitStatus.Podnesen || u.Status == UpitStatus.UObradi);
                        model.BrojZatvorenihUpita = await _context.PodrskaUpiti
                            .CountAsync(u => u.Status == UpitStatus.Zatvoren);

                        model.BrojOtvorenihBugova = await _context.BugReporti
                            .CountAsync(u => u.Status == BugStatus.Podnesen || u.Status == BugStatus.UObradi);
                        model.BrojZatvorenihBugova = await _context.BugReporti
                            .CountAsync(u => u.Status == BugStatus.Zatvoren);

                        model.TodoAkcijeSluzba = new List<string>();

                        if (model.BrojNerijesenihZahtjeva > 0)
                            model.TodoAkcijeSluzba.Add($"Obradite <b>{model.BrojNerijesenihZahtjeva}</b> neriješenih zahtjeva studenata.");

                        if (model.BrojOtvorenihUpita > 0)
                            model.TodoAkcijeSluzba.Add($"Odgovorite na <b>{model.BrojOtvorenihUpita}</b> otvorenih upita korisnika.");

                        if (model.BrojOtvorenihBugova > 0)
                            model.TodoAkcijeSluzba.Add($"Provjerite <b>{model.BrojOtvorenihBugova}</b> otvorenih prijava bugova ili prijedloga za poboljšanja.");

                        if (model.BrojUpisanihStudenata == 0)
                            model.TodoAkcijeSluzba.Add("Nema upisanih studenata. Provjerite upisni rok.");

                        if (model.BrojMojihBugova > 10) // primjer za “nagomilane” bugove
                            model.TodoAkcijeSluzba.Add("Veliki broj prijava bugova čeka na rješavanje – razmotrite hitnu reakciju!");

                        if (model.BrojOtvorenihUpita > 0 && model.BrojOtvorenihUpita > 5)
                            model.TodoAkcijeSluzba.Add("Broj otvorenih korisničkih upita je iznad normale. Provjerite raspodjelu zadataka u službi.");

                        break;
                    }
            }

            // Sistemske notifikacije (primjer: zadnje 3)
            model.ZadnjeNotifikacije = await _context.Obavjestenja
                .OrderByDescending(o => o.DatumObjave)
                .Take(3)
                .Select(o => new SystemNotifikacijaVM
                {
                    Vrijeme = o.DatumObjave,
                    Sadrzaj = o.Sadrzaj,
                    Tip = "info"
                }).ToListAsync();

            return View(model);
        }

        [HttpGet("Raspored")]
        [Authorize(Roles = "Student,Profesor,Asistent")]
        public async Task<IActionResult> Raspored()
        {
            var uloga = User.IsInRole("Student") ? "Student"
                     : User.IsInRole("Profesor") ? "Profesor"
                     : User.IsInRole("Asistent") ? "Asistent"
                     : null;

            if (uloga == null)
            {
                _logger.LogWarning("Neautorizovan pristup rasporedu.");
                return Forbid();
            }

            var aspNetUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(aspNetUserId)) return Forbid();

            long? korisnikId = null;

            if (uloga == "Student")
            {
                korisnikId = await _context.Studenti
                    .Where(s => s.AspNetUserId == aspNetUserId)
                    .Select(s => (long?)s.Id)
                    .FirstOrDefaultAsync();
            }
            else if (uloga == "Profesor")
            {
                korisnikId = await _context.Profesori
                    .Where(p => p.AspNetUserId == aspNetUserId)
                    .Select(p => (long?)p.Id)
                    .FirstOrDefaultAsync();
            }
            else if (uloga == "Asistent")
            {
                korisnikId = await _context.Asistenti
                    .Where(a => a.AspNetUserId == aspNetUserId)
                    .Select(a => (long?)a.Id)
                    .FirstOrDefaultAsync();
            }

            if (korisnikId == null) return Forbid();

            List<TerminNastave> termini = new();

            if (uloga == "Student")
            {
                var student = await _context.Studenti
                    .Include(s => s.StudentStudijskiProgrami)
                    .FirstOrDefaultAsync(s => s.Id == korisnikId);

                if (student != null)
                {
                    var programIds = student.StudentStudijskiProgrami
                        .Select(sp => sp.StudijskiProgramId)
                        .ToList();

                    termini = await _context.TerminiNastave
                        .Include(t => t.Predmet)
                        .Include(t => t.Raspored)
                                .ThenInclude(r => r.StudijskiProgram)
                        .Where(t => t.Raspored != null && programIds.Contains(t.Raspored.StudijskiProgramId))
                        .ToListAsync();
                }
            }
            else if (uloga == "Profesor")
            {
                termini = await _context.TerminiNastave
                    .Include(t => t.Predmet)
                    .Include(t => t.Raspored)
                            .ThenInclude(r => r.StudijskiProgram)
                    .Where(t => t.Predmet.ProfesorId == korisnikId)
                    .ToListAsync();
            }
            else if (uloga == "Asistent")
            {
                termini = await _context.TerminiNastave
                    .Include(t => t.Predmet)
                    .Include(t => t.Raspored)
                            .ThenInclude(r => r.StudijskiProgram)
                    .Where(t => t.Predmet.AsistentId == korisnikId)
                    .ToListAsync();
            }

            var viewModel = new MojRasporedViewModel
            {
                Termini = termini.OrderBy(t => t.Dan).ThenBy(t => t.VrijemeOd).ToList(),
                Uloga = uloga
            };

            return View(viewModel);
        }

        [HttpGet("ZahtjeviZaPrisustvo")]
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> ZahtjeviZaPrisustvo()
        {
            var aspNetUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(aspNetUserId)) return Forbid();

            long? korisnikId = null;

            if (User.IsInRole("Profesor"))
            {
                korisnikId = await _context.Profesori
                    .Where(p => p.AspNetUserId == aspNetUserId)
                    .Select(p => (long?)p.Id)
                    .FirstOrDefaultAsync();
            }
            else if (User.IsInRole("Asistent"))
            {
                korisnikId = await _context.Asistenti
                    .Where(a => a.AspNetUserId == aspNetUserId)
                    .Select(a => (long?)a.Id)
                    .FirstOrDefaultAsync();
            }

            if (korisnikId == null) return Forbid();

            var zahtjevi = await _context.ZahtjeviZaPrisustvo
                .Where(z => !z.Obradjen &&
                    ((User.IsInRole("Profesor") && z.NastavnaAktivnost.Predmet.ProfesorId == korisnikId) ||
                     (User.IsInRole("Asistent") && z.NastavnaAktivnost.Predmet.AsistentId == korisnikId)))
                .Include(z => z.Student)
                .Include(z => z.NastavnaAktivnost).ThenInclude(na => na.Predmet)
                .OrderByDescending(z => z.VrijemePodnosenja)
                .ToListAsync();

            return View(zahtjevi);
        }

        [HttpPost("ObradiOznaceneZahtjeve")]
        [Authorize(Roles = "Profesor,Asistent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ObradiOznaceneZahtjeve(List<long> oznaceniZahtjevi)
        {
            if (oznaceniZahtjevi == null || !oznaceniZahtjevi.Any())
            {
                TempData["Poruka"] = "Niste označili nijedan zahtjev za obradu.";
                return RedirectToAction("ZahtjeviZaPrisustvo");
            }

            var aspNetUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(aspNetUserId)) return Forbid();

            long? korisnikId = null;

            if (User.IsInRole("Profesor"))
            {
                korisnikId = await _context.Profesori
                    .Where(p => p.AspNetUserId == aspNetUserId)
                    .Select(p => (long?)p.Id)
                    .FirstOrDefaultAsync();
            }
            else if (User.IsInRole("Asistent"))
            {
                korisnikId = await _context.Asistenti
                    .Where(a => a.AspNetUserId == aspNetUserId)
                    .Select(a => (long?)a.Id)
                    .FirstOrDefaultAsync();
            }

            if (korisnikId == null) return Forbid();

            var zahtjevi = await _context.ZahtjeviZaPrisustvo
                .Where(z => oznaceniZahtjevi.Contains(z.Id) &&
                            !z.Obradjen &&
                           ((User.IsInRole("Profesor") && z.NastavnaAktivnost.Predmet.ProfesorId == korisnikId) ||
                            (User.IsInRole("Asistent") && z.NastavnaAktivnost.Predmet.AsistentId == korisnikId)))
                .Include(z => z.NastavnaAktivnost)
                .ToListAsync();

            foreach (var zahtjev in zahtjevi)
            {
                var key = $"napomena_{zahtjev.Id}";
                var napomena = Request.Form[key];

                if (!string.IsNullOrWhiteSpace(napomena))
                {
                    zahtjev.Odbijen = true;
                    zahtjev.Napomena = napomena;
                }
                else
                {
                    var postojiPrisustvo = await _context.PrisustvaNaAktivnostima.AnyAsync(p =>
                        p.StudentId == zahtjev.StudentId &&
                        p.NastavnaAktivnostId == zahtjev.NastavnaAktivnostId);

                    if (!postojiPrisustvo)
                    {
                        _context.PrisustvaNaAktivnostima.Add(new PrisustvoNaAktivnosti
                        {
                            StudentId = zahtjev.StudentId,
                            NastavnaAktivnostId = zahtjev.NastavnaAktivnostId,
                            VrijemeEvidentiranja = DateTime.Now
                        });
                    }

                    zahtjev.Odbijen = false;
                    zahtjev.Napomena = null;
                }

                zahtjev.Obradjen = true;
            }

            await _context.SaveChangesAsync();
            TempData["Poruka"] = $"Obrađeno zahtjeva: {zahtjevi.Count}";

            return RedirectToAction("ZahtjeviZaPrisustvo");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfilnaSlika(IFormFile profilnaSlika)
        {
            if (profilnaSlika == null || profilnaSlika.Length == 0)
            {
                TempData["Poruka"] = "Niste izabrali sliku!";
                return RedirectToAction("Index");
            }

            // Get korisnikId iz logovanog korisnika
            var aspNetUserId = _userManager.GetUserId(User);
            var korisnik = await _context.Korisnici.FirstOrDefaultAsync(k => k.AspNetUserId == aspNetUserId);
            if (korisnik == null)
                return Forbid();

            // Spremi sliku na server
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profili");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, $"{korisnik.Id}.jpg");

            // (opcionalno: validacija ekstenzije/mime tipa)
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profilnaSlika.CopyToAsync(stream);
            }

            TempData["Poruka"] = "Profilna slika uspješno ažurirana!";
            return RedirectToAction("Index");
        }


        private string GenerisiQrKodBase64(Korisnik korisnik)
        {
            var payload = $"{korisnik.Uloga}:{korisnik.Id}:{korisnik.Ime} {korisnik.Prezime}";
            var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            var pngQrCode = new PngByteQRCode(qrData);
            byte[] qrCodeBytes = pngQrCode.GetGraphic(10);
            return Convert.ToBase64String(qrCodeBytes);
        }
    }
}
