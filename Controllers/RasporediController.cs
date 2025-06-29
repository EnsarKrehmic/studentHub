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
                PauzaMin = 0
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
            {
                ModelState.AddModelError("", "Morate odabrati bar jednu lokaciju.");
            }
            if (model.Dani == null || !model.Dani.Any())
            {
                ModelState.AddModelError("", "Morate odabrati bar jedan dan.");
            }
            if (!ModelState.IsValid)
            {
                // ponovo popuni SveLokacije
                model.SveLokacije = await _context.TerminiNastave.Select(t => t.Lokacija).Distinct().OrderBy(x => x).ToListAsync();
                return View(model);
            }

            // Predmeti ovog programa i semestra
            var predmeti = await _context.Predmeti
                .Where(p => p.StudijskiProgramId == raspored.StudijskiProgramId && p.Semestar == raspored.Semestar)
                .OrderBy(p => p.Naziv)
                .ToListAsync();

            if (!predmeti.Any())
            {
                TempData["Error"] = "Nema predmeta za automatsko zakazivanje.";
                return RedirectToAction("Details", new { id });
            }

            // Dohvati SVE termine u ISTOM semestru
            var sviTerminiSemestra = await _context.TerminiNastave
                .Include(t => t.Predmet)
                .Include(t => t.Raspored)
                .Where(t => t.Raspored.Semestar == raspored.Semestar)
                .ToListAsync();

            // Generisanje termina prema pravilima iz modela
            var sati = Enumerable.Range(model.SatOd, model.SatDo - model.SatOd)
                .Select(h => new TimeSpan(h, 0, 0))
                .ToList();

            var noviTermini = new List<TerminNastave>();
            var izvjestaj = new List<string>();
            int dodano = 0;

            foreach (var predmet in predmeti)
            {
                foreach (var vrsta in new[] { VrstaNastave.Predavanje, VrstaNastave.Vjezbe })
                {
                    bool terminDodijeljen = false;

                    foreach (var dan in model.Dani)
                    {
                        foreach (var sat in sati)
                        {
                            var kraj = sat + TimeSpan.FromMinutes(model.TrajanjeMin);

                            // Petak popodne (opcionalno izbjegavanje)
                            if (model.NeRasporedjujPetkomPopodne && dan == DayOfWeek.Friday && sat.Hours >= 14)
                                continue;

                            foreach (var lokacija in model.OdabraneLokacije)
                            {
                                // PREKLAPANJE: Lokacija
                                bool preklapanjeLokacija = sviTerminiSemestra.Any(t =>
                                    t.Lokacija == lokacija &&
                                    t.Dan == dan &&
                                    (
                                        (sat >= t.VrijemeOd && sat < t.VrijemeDo) ||
                                        (kraj > t.VrijemeOd && kraj <= t.VrijemeDo) ||
                                        (sat <= t.VrijemeOd && kraj >= t.VrijemeDo)
                                    ));

                                // PREKLAPANJE: Profesor/Asistent
                                bool preklapanjeNastavnika = sviTerminiSemestra.Any(t =>
                                    t.Dan == dan &&
                                    (
                                        (sat >= t.VrijemeOd && sat < t.VrijemeDo) ||
                                        (kraj > t.VrijemeOd && kraj <= t.VrijemeDo) ||
                                        (sat <= t.VrijemeOd && kraj >= t.VrijemeDo)
                                    ) &&
                                    (
                                        (vrsta == VrstaNastave.Predavanje &&
                                            t.Predmet != null && predmet.ProfesorId != null && t.Predmet.ProfesorId == predmet.ProfesorId) ||
                                        (vrsta == VrstaNastave.Vjezbe &&
                                            t.Predmet != null && predmet.AsistentId != null && t.Predmet.AsistentId == predmet.AsistentId)
                                    ));

                                // PREKLAPANJE: Studenti na predmetu
                                bool preklapanjeStudenta = false;
                                var studentiPredmeta = await _context.StudentiNaPredmetima
                                    .Where(sp => sp.PredmetId == predmet.Id)
                                    .Select(sp => sp.StudentId)
                                    .ToListAsync();

                                if (studentiPredmeta.Any())
                                {
                                    preklapanjeStudenta = sviTerminiSemestra.Any(t =>
                                        t.Dan == dan &&
                                        (
                                            (sat >= t.VrijemeOd && sat < t.VrijemeDo) ||
                                            (kraj > t.VrijemeOd && kraj <= t.VrijemeDo) ||
                                            (sat <= t.VrijemeOd && kraj >= t.VrijemeDo)
                                        ) &&
                                        t.PredmetId != predmet.Id &&
                                        _context.StudentiNaPredmetima.Any(sp => studentiPredmeta.Contains(sp.StudentId) && sp.PredmetId == t.PredmetId)
                                    );
                                }

                                // Izbjegavaj uzastopne termine istom nastavniku (opcija)
                                bool uzastopniNastavnik = false;
                                if (model.IzbjegavajUzastopneTermine && (predmet.ProfesorId.HasValue || predmet.AsistentId.HasValue))
                                {
                                    uzastopniNastavnik = sviTerminiSemestra.Any(t =>
                                        t.Dan == dan &&
                                        (
                                            (t.VrijemeOd + TimeSpan.FromMinutes(model.TrajanjeMin + model.PauzaMin) == sat) ||
                                            (sat + TimeSpan.FromMinutes(model.TrajanjeMin + model.PauzaMin) == t.VrijemeOd)
                                        ) &&
                                        (
                                            (vrsta == VrstaNastave.Predavanje &&
                                                t.Predmet != null && t.Predmet.ProfesorId == predmet.ProfesorId) ||
                                            (vrsta == VrstaNastave.Vjezbe &&
                                                t.Predmet != null && t.Predmet.AsistentId == predmet.AsistentId)
                                        ));
                                }

                                if (!preklapanjeLokacija && !preklapanjeNastavnika && !preklapanjeStudenta && !uzastopniNastavnik)
                                {
                                    var termin = new TerminNastave
                                    {
                                        PredmetId = predmet.Id,
                                        Vrsta = vrsta,
                                        Dan = dan,
                                        VrijemeOd = sat,
                                        VrijemeDo = kraj,
                                        Lokacija = lokacija,
                                        RasporedId = raspored.Id
                                    };

                                    noviTermini.Add(termin);
                                    sviTerminiSemestra.Add(termin); // Dodaj u listu da blokira buduće slotove
                                    dodano++;
                                    izvjestaj.Add($"✅ {predmet.Naziv} ({vrsta}) - {lokacija}, {dan}, {sat:hh\\:mm}-{kraj:hh\\:mm}");
                                    terminDodijeljen = true;
                                    break;
                                }
                                else
                                {
                                    string razlog = "";
                                    if (preklapanjeLokacija) razlog += "Lokacija zauzeta. ";
                                    if (preklapanjeNastavnika) razlog += "Profesor/asistent zauzet. ";
                                    if (preklapanjeStudenta) razlog += "Student zauzet. ";
                                    if (uzastopniNastavnik) razlog += "Uzastopni termin za nastavnika. ";
                                    izvjestaj.Add($"⚠️ {predmet.Naziv} ({vrsta}) - {lokacija}, {dan}, {sat:hh\\:mm}-{kraj:hh\\:mm} | {razlog}");
                                }
                            }
                            if (terminDodijeljen) break;
                        }
                        if (terminDodijeljen) break;
                    }
                }
            }

            if (!noviTermini.Any())
            {
                TempData["Error"] = "Nije bilo moguće automatski dodijeliti niti jedan termin bez preklapanja.";
                return RedirectToAction("Details", new { id });
            }

            _context.TerminiNastave.AddRange(noviTermini);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Automatski generisano {dodano} termina (predavanja i vježbi).<br><pre>{string.Join("\n", izvjestaj)}</pre>";
            return RedirectToAction("Details", new { id });
        }
    }
}