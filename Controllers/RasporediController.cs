using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;

namespace StudentHub.Controllers
{
    [Route("Rasporedi")]
    public class RasporediController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RasporediController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public RasporediController(ApplicationDbContext context, ILogger<RasporediController> logger, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var rasporedi = await _context.Rasporedi
                .Include(r => r.StudijskiProgram)
                .ToListAsync();

            _logger.LogInformation("Učitana lista svih rasporeda ({BrojRasporeda} pronađeno).", rasporedi.Count);

            return View(rasporedi);
        }

        [HttpGet("Details/{id:long}")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Pozvan Details bez ID parametra.");
                return NotFound();
            }

            var raspored = await _context.Rasporedi
                .Include(r => r.StudijskiProgram)
                .Include(r => r.Termini)
                    .ThenInclude(t => t.Predmet)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (raspored == null)
            {
                _logger.LogWarning("Raspored sa ID {RasporedId} nije pronađen.", id);
                return NotFound();
            }

            var sviTermini = raspored.Termini
                .OrderBy(t => t.Dan)
                .ThenBy(t => t.VrijemeOd)
                .ToList();

            var uloga = User.IsInRole("Studentska služba") ? "Studentska služba"
                     : User.IsInRole("Profesor") ? "Profesor"
                     : User.IsInRole("Asistent") ? "Asistent"
                     : User.IsInRole("Student") ? "Student"
                     : "Nepoznata";

            long? korisnikId = null;
            var aspNetUserId = _userManager.GetUserId(User);

            if (!string.IsNullOrEmpty(aspNetUserId))
            {
                if (uloga == "Profesor")
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
                else if (uloga == "Student")
                {
                    korisnikId = await _context.Studenti
                        .Where(s => s.AspNetUserId == aspNetUserId)
                        .Select(s => (long?)s.Id)
                        .FirstOrDefaultAsync();
                }
            }

            var prikazaniTermini = sviTermini;

            if (uloga == "Profesor")
            {
                prikazaniTermini = sviTermini
                    .Where(t => t.Predmet.ProfesorId == korisnikId)
                    .ToList();
            }
            else if (uloga == "Asistent")
            {
                prikazaniTermini = sviTermini
                    .Where(t => t.Predmet.AsistentId == korisnikId)
                    .ToList();
            }
            else if (uloga == "Student")
            {
                var student = await _context.Studenti
                    .Include(s => s.StudentStudijskiProgrami)
                    .FirstOrDefaultAsync(s => s.Id == korisnikId);

                if (student != null)
                {
                    var pripadaProgramu = student.StudentStudijskiProgrami
                        .Any(sp => sp.StudijskiProgramId == raspored.StudijskiProgramId);

                    prikazaniTermini = pripadaProgramu ? sviTermini : new List<TerminNastave>();
                }
                else
                {
                    prikazaniTermini = new List<TerminNastave>();
                }
            }

            var viewModel = new RasporedDetailsViewModel
            {
                Raspored = raspored,
                Termini = prikazaniTermini,
                PrikazSamoLični = uloga != "Studentska služba",
                KorisnickaUloga = uloga,
                KorisnikId = korisnikId
            };

            _logger.LogInformation("Prikaz detalja rasporeda ID {Id} za ulogu {Uloga}, korisnik ID {KorisnikId}.", id, uloga, korisnikId);

            return View(viewModel);
        }

        [HttpGet("Create")]
        [Authorize(Roles = "Studentska služba")]
        public IActionResult Create()
        {
            ViewBag.StudijskiProgrami = _context.StudijskiProgrami.ToList();
            return View(new Raspored());
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Create([Bind("StudijskiProgramId,GodinaStudija,Semestar,AkademskaGodina")] Raspored raspored)
        {
            // Validacija unikatnosti: program + godina studija + semestar + akademska godina
            bool postoji = await _context.Rasporedi.AnyAsync(r =>
                r.StudijskiProgramId == raspored.StudijskiProgramId &&
                r.GodinaStudija == raspored.GodinaStudija &&
                r.Semestar == raspored.Semestar &&
                r.AkademskaGodina == raspored.AkademskaGodina
            );

            if (postoji)
            {
                ModelState.AddModelError("", "Raspored za odabrani program, godinu studija, semestar i akademsku godinu već postoji! Ako želite uređivati postojeći raspored, pronađite ga u listi.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(raspored);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Kreiran je novi raspored ID {Id} za program ID {ProgramId}, godina {Godina}, semestar {Semestar}.",
                        raspored.Id, raspored.StudijskiProgramId, raspored.GodinaStudija, raspored.Semestar);

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Greška prilikom dodavanja novog rasporeda.");
                    ModelState.AddModelError("", "Došlo je do greške prilikom spremanja rasporeda.");
                }
            }
            else
            {
                _logger.LogWarning("ModelState nije validan prilikom kreiranja rasporeda.");

                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Any())
                    {
                        var errors = string.Join(", ", state.Errors.Select(e => e.ErrorMessage));
                        _logger.LogWarning("Polje '{Field}' nije validno: {Errors}", key, errors);
                    }
                    else if (state.RawValue == null)
                    {
                        _logger.LogWarning("Polje '{Field}' nije poslato u formi ili je null.", key);
                    }
                }
            }

            ViewBag.StudijskiProgrami = _context.StudijskiProgrami.ToList();
            return View(raspored);
        }

        [HttpGet("Edit/{id:long}")]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Pozvan Edit bez ID parametra.");
                return NotFound();
            }

            var raspored = await _context.Rasporedi.FindAsync(id);
            if (raspored == null)
            {
                _logger.LogWarning("Raspored sa ID {Id} nije pronađen za uređivanje.", id);
                return NotFound();
            }

            ViewBag.StudijskiProgrami = _context.StudijskiProgrami.ToList();
            return View(raspored);
        }

        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Edit(long id, [Bind("Id,StudijskiProgramId,GodinaStudija,Semestar,AkademskaGodina")] Raspored raspored)
        {
            if (id != raspored.Id)
            {
                _logger.LogWarning("Neusklađenost ID-a pri uređivanju rasporeda: {Id} != {RasporedId}", id, raspored.Id);
                return NotFound();
            }

            // Validacija unikatnosti: ignorisi trenutni raspored
            bool postoji = await _context.Rasporedi.AnyAsync(r =>
                r.Id != raspored.Id &&
                r.StudijskiProgramId == raspored.StudijskiProgramId &&
                r.GodinaStudija == raspored.GodinaStudija &&
                r.Semestar == raspored.Semestar &&
                r.AkademskaGodina == raspored.AkademskaGodina
            );

            if (postoji)
            {
                ModelState.AddModelError("", "Već postoji raspored za odabrani program, godinu studija, semestar i akademsku godinu.");
            }
            if (id != raspored.Id)
            {
                _logger.LogWarning("Neusklađenost ID-a pri uređivanju rasporeda: {Id} != {RasporedId}", id, raspored.Id);
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(raspored);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Uređen raspored ID {Id}.", raspored.Id);
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Greška prilikom uređivanja rasporeda ID {Id}.", raspored.Id);

                    if (!_context.Rasporedi.Any(e => e.Id == id))
                        return NotFound();

                    throw;
                }
            }
            else
            {
                _logger.LogWarning("ModelState nije validan prilikom uređivanja rasporeda ID {Id}.", raspored.Id);

                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Any())
                    {
                        var errors = string.Join(", ", state.Errors.Select(e => e.ErrorMessage));
                        _logger.LogWarning("Polje '{Field}' nije validno: {Errors}", key, errors);
                    }
                    else if (state.RawValue == null)
                    {
                        _logger.LogWarning("Polje '{Field}' nije poslato u formi ili je null.", key);
                    }
                }
            }

            ViewBag.StudijskiProgrami = _context.StudijskiProgrami.ToList();
            return View(raspored);
        }

        [HttpGet("Delete/{id:long}")]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
                return NotFound();

            var raspored = await _context.Rasporedi
                .Include(r => r.StudijskiProgram)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (raspored == null)
            {
                _logger.LogWarning("Pokušaj brisanja nepostojećeg rasporeda ID {Id}.", id);
                return NotFound();
            }

            return View(raspored);
        }

        [HttpPost("Delete/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            try
            {
                var raspored = await _context.Rasporedi
                    .Include(r => r.Termini)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (raspored != null)
                {
                    // Prvo brišemo povezane termine
                    _context.TerminiNastave.RemoveRange(raspored.Termini);

                    // Zatim brišemo raspored
                    _context.Rasporedi.Remove(raspored);

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Uspješno obrisan raspored ID {Id} sa svim terminima.", id);
                }
                else
                {
                    _logger.LogWarning("Nema rasporeda za brisanje sa ID {Id}.", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška prilikom brisanja rasporeda ID {Id}.", id);
                ModelState.AddModelError("", "Došlo je do greške prilikom brisanja rasporeda.");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("GenerisiTermine/{id:long}")]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> GenerisiTermine(long id)
        {
            var raspored = await _context.Rasporedi
                .Include(r => r.StudijskiProgram)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (raspored == null)
            {
                TempData["Error"] = "Raspored nije pronađen.";
                return RedirectToAction("Details", new { id });
            }

            // Sve moguće lokacije (učionice)
            var sveLokacije = await _context.TerminiNastave
                .Select(t => t.Lokacija)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            if (!sveLokacije.Any())
                sveLokacije = new List<string> { "4201", "4202", "4203", "Nemila" };

            var daniDefault = new List<DayOfWeek>
    {
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday
    };

            var viewModel = new GenerisanjeTerminaViewModel
            {
                RasporedId = id,
                SveLokacije = sveLokacije,
                OdabraneLokacije = sveLokacije.ToList(),
                Dani = daniDefault,
                SatOd = 8,
                SatDo = 18,
                TrajanjeMin = 45,
                PauzaMin = 0,
                BrojSedmica = 15 // NOVO polje, možeš staviti i u ViewModel za unos
            };

            return View(viewModel);
        }

        [HttpPost("GenerisiTermine/{id:long}")]
        [Authorize(Roles = "Studentska služba")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerisiTermine(long id, GenerisanjeTerminaViewModel model)
        {
            var raspored = await _context.Rasporedi
                .Include(r => r.StudijskiProgram)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (raspored == null)
            {
                TempData["Error"] = "Raspored nije pronađen.";
                return RedirectToAction("Details", new { id });
            }

            // Validacija ulaznih vrijednosti
            if (model.OdabraneLokacije == null || !model.OdabraneLokacije.Any())
                ModelState.AddModelError("", "Morate odabrati bar jednu lokaciju.");
            if (model.Dani == null || !model.Dani.Any())
                ModelState.AddModelError("", "Morate odabrati bar jedan dan.");

            if (model.BrojSedmica <= 0) model.BrojSedmica = 15;

            if (!ModelState.IsValid)
            {
                model.SveLokacije = await _context.TerminiNastave.Select(t => t.Lokacija).Distinct().OrderBy(x => x).ToListAsync();
                return View(model);
            }

            var predmeti = await _context.Predmeti
                .Where(p =>
                    p.StudijskiProgramId == raspored.StudijskiProgramId &&
                    p.Semestar == raspored.Semestar &&
                    p.GodinaStudija == raspored.GodinaStudija
                    )
                    .OrderBy(p => p.Naziv)
                .ToListAsync();

            if (!predmeti.Any())
            {
                TempData["Error"] = "Nema predmeta za automatsko zakazivanje.";
                return RedirectToAction("Details", new { id });
            }

            var sviTerminiSemestra = await _context.TerminiNastave
                .Include(t => t.Predmet)
                .Include(t => t.Raspored)
                .Where(t => t.Raspored.Semestar == raspored.Semestar)
                .ToListAsync();

            var sati = Enumerable.Range(model.SatOd, model.SatDo - model.SatOd)
                .Select(h => new TimeSpan(h, 0, 0))
                .ToList();

            var noviTermini = new List<TerminNastave>();
            var izvjestaj = new List<string>();
            int dodano = 0;
            var summary = new List<string>();

            foreach (var predmet in predmeti)
            {
                // --- BROJ TERMINA PREDAVANJA SEDMIČNO ---
                int ukupnoPredavanja = predmet.SatiPredavanja;
                double satiSedmicnoPredavanja = ukupnoPredavanja / (double)model.BrojSedmica;
                int trajanjePredavanjaMin = model.TrajanjeMin;
                int trajanjePredavanjaSat = trajanjePredavanjaMin / 60;
                // koliko termina po sedmici treba (zaokružujemo na više)
                int brojTerminaPredavanjaSedmicno = (int)Math.Ceiling((satiSedmicnoPredavanja * 60) / trajanjePredavanjaMin);

                // --- BROJ TERMINA VJEŽBI SEDMIČNO ---
                int ukupnoVjezbi = predmet.SatiVjezbi;
                double satiSedmicnoVjezbe = ukupnoVjezbi / (double)model.BrojSedmica;
                int brojTerminaVjezbeSedmicno = (int)Math.Ceiling((satiSedmicnoVjezbe * 60) / trajanjePredavanjaMin);

                // --- PAMETNA RASPODJELA DANA ---
                var daniZaPredavanja = model.Dani.Take(brojTerminaPredavanjaSedmicno).ToList();
                var daniZaVjezbe = model.Dani.Skip(brojTerminaPredavanjaSedmicno).Take(brojTerminaVjezbeSedmicno).ToList();

                // --- GENERISANJE TERMINA PREDAVANJA ---
                int satiOstaliPredavanja = ukupnoPredavanja;
                for (int i = 0; i < brojTerminaPredavanjaSedmicno; i++)
                {
                    // Pravi "čudne" sate: ako nije djeljivo s trajanjem termina, zadnji termin može biti kraći
                    int trajanjeOvajTermin = Math.Min(trajanjePredavanjaMin, satiOstaliPredavanja * 60);
                    if (i == brojTerminaPredavanjaSedmicno - 1 && (ukupnoPredavanja * 60) % trajanjePredavanjaMin != 0)
                        trajanjeOvajTermin = (ukupnoPredavanja * 60) - (trajanjePredavanjaMin * (brojTerminaPredavanjaSedmicno - 1));

                    var dan = daniZaPredavanja.Count > i ? daniZaPredavanja[i] : model.Dani[i % model.Dani.Count];

                    bool dodanoPredavanje = false;
                    foreach (var sat in sati)
                    {
                        var kraj = sat + TimeSpan.FromMinutes(trajanjeOvajTermin);

                        foreach (var lokacija in model.OdabraneLokacije)
                        {
                            // --- PREKLAPANJA/PROVJERE (isti kao do sada) ---
                            bool preklapanje = sviTerminiSemestra.Any(t =>
                                t.Lokacija == lokacija && t.Dan == dan &&
                                (
                                    (sat >= t.VrijemeOd && sat < t.VrijemeDo) ||
                                    (kraj > t.VrijemeOd && kraj <= t.VrijemeDo) ||
                                    (sat <= t.VrijemeOd && kraj >= t.VrijemeDo)
                                )
                            );
                            if (preklapanje) continue;

                            var termin = new TerminNastave
                            {
                                PredmetId = predmet.Id,
                                Vrsta = VrstaNastave.Predavanje,
                                Dan = dan,
                                VrijemeOd = sat,
                                VrijemeDo = kraj,
                                Lokacija = lokacija,
                                RasporedId = raspored.Id
                            };
                            noviTermini.Add(termin);
                            sviTerminiSemestra.Add(termin);
                            dodano++;
                            izvjestaj.Add($"✅ {predmet.Naziv} (Predavanje, {trajanjeOvajTermin} min) - {lokacija}, {dan}, {sat:hh\\:mm}-{kraj:hh\\:mm}");
                            dodanoPredavanje = true;
                            break;
                        }
                        if (dodanoPredavanje) break;
                    }
                    satiOstaliPredavanja -= trajanjeOvajTermin / 60;
                }
                summary.Add($"📘 Predmet **{predmet.Naziv}**: {brojTerminaPredavanjaSedmicno} termina predavanja sedmično × {model.BrojSedmica} = {brojTerminaPredavanjaSedmicno * model.BrojSedmica} termina. ({ukupnoPredavanja} sati predavanja ukupno)");

                // --- GENERISANJE TERMINA VJEŽBI ---
                int satiOstaliVjezbe = ukupnoVjezbi;
                for (int i = 0; i < brojTerminaVjezbeSedmicno; i++)
                {
                    int trajanjeOvajTermin = Math.Min(trajanjePredavanjaMin, satiOstaliVjezbe * 60);
                    if (i == brojTerminaVjezbeSedmicno - 1 && (ukupnoVjezbi * 60) % trajanjePredavanjaMin != 0)
                        trajanjeOvajTermin = (ukupnoVjezbi * 60) - (trajanjePredavanjaMin * (brojTerminaVjezbeSedmicno - 1));

                    var dan = daniZaVjezbe.Count > i ? daniZaVjezbe[i] : model.Dani[(i + brojTerminaPredavanjaSedmicno) % model.Dani.Count];

                    bool dodanoVjezba = false;
                    foreach (var sat in sati)
                    {
                        var kraj = sat + TimeSpan.FromMinutes(trajanjeOvajTermin);

                        foreach (var lokacija in model.OdabraneLokacije)
                        {
                            bool preklapanje = sviTerminiSemestra.Any(t =>
                                t.Lokacija == lokacija && t.Dan == dan &&
                                (
                                    (sat >= t.VrijemeOd && sat < t.VrijemeDo) ||
                                    (kraj > t.VrijemeOd && kraj <= t.VrijemeDo) ||
                                    (sat <= t.VrijemeOd && kraj >= t.VrijemeDo)
                                )
                            );
                            if (preklapanje) continue;

                            var termin = new TerminNastave
                            {
                                PredmetId = predmet.Id,
                                Vrsta = VrstaNastave.Vjezbe,
                                Dan = dan,
                                VrijemeOd = sat,
                                VrijemeDo = kraj,
                                Lokacija = lokacija,
                                RasporedId = raspored.Id
                            };
                            noviTermini.Add(termin);
                            sviTerminiSemestra.Add(termin);
                            dodano++;
                            izvjestaj.Add($"✅ {predmet.Naziv} (Vježbe, {trajanjeOvajTermin} min) - {lokacija}, {dan}, {sat:hh\\:mm}-{kraj:hh\\:mm}");
                            dodanoVjezba = true;
                            break;
                        }
                        if (dodanoVjezba) break;
                    }
                    satiOstaliVjezbe -= trajanjeOvajTermin / 60;
                }
                summary.Add($"🟢 {brojTerminaVjezbeSedmicno} termina vježbi sedmično × {model.BrojSedmica} = {brojTerminaVjezbeSedmicno * model.BrojSedmica} termina. ({ukupnoVjezbi} sati vježbi ukupno)");

                // --- DODATNA UPOZORENJA ZA "ČUDNE" SATE ---
                if ((ukupnoPredavanja * 60) % trajanjePredavanjaMin != 0)
                    summary.Add($"⚠️ Predmet {predmet.Naziv} ima netipičan broj sati predavanja, zadnji termin može biti kraći ({(ukupnoPredavanja * 60) % trajanjePredavanjaMin} min).");
                if ((ukupnoVjezbi * 60) % trajanjePredavanjaMin != 0)
                    summary.Add($"⚠️ Predmet {predmet.Naziv} ima netipičan broj sati vježbi, zadnji termin može biti kraći ({(ukupnoVjezbi * 60) % trajanjePredavanjaMin} min).");
            }

            if (!noviTermini.Any())
            {
                TempData["Error"] = "Nije bilo moguće automatski dodijeliti niti jedan termin bez preklapanja.";
                return RedirectToAction("Details", new { id });
            }

            _context.TerminiNastave.AddRange(noviTermini);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"<strong>Sažetak:</strong><br><pre>{string.Join("\n", summary)}</pre>"
                + $"<br><strong>Detalji termina:</strong><br><pre>{string.Join("\n", izvjestaj)}</pre>";

            return RedirectToAction("Details", new { id });
        }
    }
}