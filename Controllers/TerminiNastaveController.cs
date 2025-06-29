using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;

namespace StudentHub.Controllers
{
    [Authorize(Roles = "Studentska služba")]
    [Route("TerminiNastave")]
    public class TerminiNastaveController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TerminiNastaveController> _logger;

        public TerminiNastaveController(ApplicationDbContext context, ILogger<TerminiNastaveController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: TerminiNastave/Create?rasporedId=...
        [HttpGet("Create")]
        public async Task<IActionResult> Create(long rasporedId)
        {
            var raspored = await _context.Rasporedi
                .Include(r => r.StudijskiProgram)
                .FirstOrDefaultAsync(r => r.Id == rasporedId);

            if (raspored == null)
            {
                _logger.LogWarning("Pokušaj kreiranja termina za nepostojeći raspored ID {RasporedId}.", rasporedId);
                return NotFound();
            }

            ViewBag.RasporedId = rasporedId;
            ViewBag.StudijskiProgramId = raspored.StudijskiProgramId;
            ViewBag.GodinaStudija = raspored.GodinaStudija;
            ViewBag.Semestar = raspored.Semestar;

            return View(new TerminNastave { RasporedId = rasporedId });
        }

        // POST: TerminiNastave/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PredmetId,Vrsta,Dan,VrijemeOd,VrijemeDo,Lokacija,RasporedId")] TerminNastave termin)
        {
            var predmet = await _context.Predmeti
                .Include(p => p.Profesor)
                .Include(p => p.Asistent)
                .FirstOrDefaultAsync(p => p.Id == termin.PredmetId);

            if (predmet == null)
            {
                ModelState.AddModelError("", "Nepostojeći predmet.");
            }

            if (ModelState.IsValid && predmet != null)
            {
                var kolizije = await _context.TerminiNastave
                    .Include(t => t.Predmet)
                    .Where(t =>
                        t.RasporedId != null &&
                        t.Dan == termin.Dan &&
                        t.VrijemeOd < termin.VrijemeDo &&
                        termin.VrijemeOd < t.VrijemeDo &&
                        (
                            t.Lokacija == termin.Lokacija
                            || (t.Predmet.ProfesorId != null && predmet.ProfesorId != null && t.Predmet.ProfesorId == predmet.ProfesorId)
                            || (t.Predmet.AsistentId != null && predmet.AsistentId != null && t.Predmet.AsistentId == predmet.AsistentId)
                            || (t.RasporedId == termin.RasporedId)
                        )
                    )
                    .ToListAsync();

                if (kolizije.Any())
                {
                    foreach (var kol in kolizije)
                    {
                        if (kol.Lokacija == termin.Lokacija)
                            ModelState.AddModelError("", $"[LOKACIJA] Prostorija '{kol.Lokacija}' je zauzeta ({kol.Predmet?.Naziv} - {kol.Vrsta}, {kol.VrijemeOd:hh\\:mm}-{kol.VrijemeDo:hh\\:mm}, {kol.Dan})!");

                        if (kol.Predmet.ProfesorId == predmet.ProfesorId && predmet.ProfesorId != null)
                            ModelState.AddModelError("", $"[PROFESOR] Profesor '{kol.Predmet.Profesor?.Ime} {kol.Predmet.Profesor?.Prezime}' već ima nastavu ({kol.Predmet?.Naziv} - {kol.Vrsta}, {kol.Lokacija}, {kol.VrijemeOd:hh\\:mm}-{kol.VrijemeDo:hh\\:mm}, {kol.Dan})!");

                        if (kol.Predmet.AsistentId == predmet.AsistentId && predmet.AsistentId != null)
                            ModelState.AddModelError("", $"[ASISTENT] Asistent '{kol.Predmet.Asistent?.Ime} {kol.Predmet.Asistent?.Prezime}' već ima nastavu ({kol.Predmet?.Naziv} - {kol.Vrsta}, {kol.Lokacija}, {kol.VrijemeOd:hh\\:mm}-{kol.VrijemeDo:hh\\:mm}, {kol.Dan})!");

                        if (kol.RasporedId == termin.RasporedId)
                            ModelState.AddModelError("", $"[STUDENTI] Studenti ovog programa/godine/semstra već imaju termin ({kol.Predmet?.Naziv} - {kol.Vrsta}, {kol.Lokacija}, {kol.VrijemeOd:hh\\:mm}-{kol.VrijemeDo:hh\\:mm}, {kol.Dan})!");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.TerminiNastave.Add(termin);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Dodan termin za predmet ID {PredmetId} u raspored ID {RasporedId}.", termin.PredmetId, termin.RasporedId);
                    return RedirectToAction("Details", "Rasporedi", new { id = termin.RasporedId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Greška prilikom kreiranja termina.");
                    ModelState.AddModelError("", "Došlo je do greške prilikom spremanja termina.");
                }
            }
            else
            {
                _logger.LogWarning("ModelState nije validan prilikom kreiranja termina.");
            }

            var raspored = await _context.Rasporedi.FirstOrDefaultAsync(r => r.Id == termin.RasporedId);

            ViewBag.RasporedId = termin.RasporedId;
            ViewBag.StudijskiProgramId = raspored?.StudijskiProgramId;
            ViewBag.GodinaStudija = raspored?.GodinaStudija;
            ViewBag.Semestar = raspored?.Semestar;

            return View(termin);
        }

        [HttpGet("Edit/{id:long}")]
        public async Task<IActionResult> Edit(long id)
        {
            var termin = await _context.TerminiNastave.FindAsync(id);
            if (termin == null)
            {
                _logger.LogWarning("Pokušaj uređivanja nepostojećeg termina ID {Id}.", id);
                return NotFound();
            }

            var raspored = await _context.Rasporedi.FirstOrDefaultAsync(r => r.Id == termin.RasporedId);

            ViewBag.Predmeti = await _context.Predmeti
                .Where(p => p.StudijskiProgramId == raspored.StudijskiProgramId
                         && p.Semestar == raspored.Semestar
                         && _context.NastavniPlanovi.Any(np => np.Id == p.NastavniPlanId
                                                               && np.StudijskiProgramId == raspored.StudijskiProgramId
                                                               && np.GodinaStudija == raspored.GodinaStudija.ToString()))
                .OrderBy(p => p.Naziv)
                .ToListAsync();

            return View(termin);
        }

        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,PredmetId,Vrsta,Dan,VrijemeOd,VrijemeDo,Lokacija,RasporedId")] TerminNastave termin)
        {
            if (id != termin.Id)
            {
                _logger.LogWarning("Neusklađeni ID-ovi prilikom uređivanja termina: {Id} != {TerminId}.", id, termin.Id);
                return NotFound();
            }

            var predmet = await _context.Predmeti
                .Include(p => p.Profesor)
                .Include(p => p.Asistent)
                .FirstOrDefaultAsync(p => p.Id == termin.PredmetId);

            if (predmet == null)
            {
                ModelState.AddModelError("", "Nepostojeći predmet.");
            }

            if (ModelState.IsValid && predmet != null)
            {
                var kolizije = await _context.TerminiNastave
                    .Include(t => t.Predmet)
                    .Where(t =>
                        t.Id != termin.Id && // Exclude self
                        t.RasporedId != null &&
                        t.Dan == termin.Dan &&
                        t.VrijemeOd < termin.VrijemeDo &&
                        termin.VrijemeOd < t.VrijemeDo &&
                        (
                            t.Lokacija == termin.Lokacija
                            || (t.Predmet.ProfesorId != null && predmet.ProfesorId != null && t.Predmet.ProfesorId == predmet.ProfesorId)
                            || (t.Predmet.AsistentId != null && predmet.AsistentId != null && t.Predmet.AsistentId == predmet.AsistentId)
                            || (t.RasporedId == termin.RasporedId)
                        )
                    )
                    .ToListAsync();

                if (kolizije.Any())
                {
                    foreach (var kol in kolizije)
                    {
                        if (kol.Lokacija == termin.Lokacija)
                            ModelState.AddModelError("", $"[LOKACIJA] Prostorija '{kol.Lokacija}' je zauzeta ({kol.Predmet?.Naziv} - {kol.Vrsta}, {kol.VrijemeOd:hh\\:mm}-{kol.VrijemeDo:hh\\:mm}, {kol.Dan})!");

                        if (kol.Predmet.ProfesorId == predmet.ProfesorId && predmet.ProfesorId != null)
                            ModelState.AddModelError("", $"[PROFESOR] Profesor '{kol.Predmet.Profesor?.Ime} {kol.Predmet.Profesor?.Prezime}' već ima nastavu ({kol.Predmet?.Naziv} - {kol.Vrsta}, {kol.Lokacija}, {kol.VrijemeOd:hh\\:mm}-{kol.VrijemeDo:hh\\:mm}, {kol.Dan})!");

                        if (kol.Predmet.AsistentId == predmet.AsistentId && predmet.AsistentId != null)
                            ModelState.AddModelError("", $"[ASISTENT] Asistent '{kol.Predmet.Asistent?.Ime} {kol.Predmet.Asistent?.Prezime}' već ima nastavu ({kol.Predmet?.Naziv} - {kol.Vrsta}, {kol.Lokacija}, {kol.VrijemeOd:hh\\:mm}-{kol.VrijemeDo:hh\\:mm}, {kol.Dan})!");

                        if (kol.RasporedId == termin.RasporedId)
                            ModelState.AddModelError("", $"[STUDENTI] Studenti ovog programa/godine/semstra već imaju termin ({kol.Predmet?.Naziv} - {kol.Vrsta}, {kol.Lokacija}, {kol.VrijemeOd:hh\\:mm}-{kol.VrijemeDo:hh\\:mm}, {kol.Dan})!");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(termin);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Uređen termin ID {Id}.", termin.Id);
                    return RedirectToAction("Details", "Rasporedi", new { id = termin.RasporedId });
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Greška prilikom uređivanja termina ID {Id}.", id);
                    throw;
                }
            }

            var raspored = await _context.Rasporedi.FirstOrDefaultAsync(r => r.Id == termin.RasporedId);

            ViewBag.Predmeti = await _context.Predmeti
                .Where(p => p.StudijskiProgramId == raspored.StudijskiProgramId
                         && p.Semestar == raspored.Semestar
                         && _context.NastavniPlanovi.Any(np => np.Id == p.NastavniPlanId
                                                               && np.StudijskiProgramId == raspored.StudijskiProgramId
                                                               && np.GodinaStudija == raspored.GodinaStudija.ToString()))
                .OrderBy(p => p.Naziv)
                .ToListAsync();

            return View(termin);
        }

        [HttpGet("Delete/{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var termin = await _context.TerminiNastave
                .Include(t => t.Predmet)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (termin == null)
            {
                _logger.LogWarning("Pokušaj brisanja nepostojećeg termina ID {Id}.", id);
                return NotFound();
            }

            return View(termin);
        }

        [HttpPost("Delete/{id:long}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var termin = await _context.TerminiNastave.FindAsync(id);
            if (termin == null)
            {
                _logger.LogWarning("Termin za brisanje nije pronađen ID {Id}.", id);
                return NotFound();
            }

            var rasporedId = termin.RasporedId ?? 0;

            try
            {
                _context.TerminiNastave.Remove(termin);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Obrisan termin ID {Id}.", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška prilikom brisanja termina ID {Id}.", id);
                ModelState.AddModelError("", "Došlo je do greške prilikom brisanja.");
                return View(termin);
            }

            return RedirectToAction("Details", "Rasporedi", new { id = rasporedId });
        }

        // AJAX: Dohvati predmete za dati program, godinu i semestar
        [HttpGet("PredmetiZaProgram")]
        public async Task<IActionResult> PredmetiZaProgram(long studijskiProgramId, int godinaStudija, int semestar)
        {
            // Mapiranje integer godine studija u string (jer u NastavniPlan je string)
            string godinaString = godinaStudija.ToString();

            var predmeti = await _context.Predmeti
                .Where(p => p.StudijskiProgramId == studijskiProgramId
                            && p.Semestar == semestar
                            && _context.NastavniPlanovi.Any(np => np.Id == p.NastavniPlanId
                                                                  && np.StudijskiProgramId == studijskiProgramId
                                                                  && np.GodinaStudija == godinaString))
                .OrderBy(p => p.Naziv)
                .Select(p => new { p.Id, p.Naziv })
                .ToListAsync();

            return Json(predmeti);
        }
    }
}
