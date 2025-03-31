using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StudentHub.Controllers
{
    [Authorize]
    public class NastavneAktivnostiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NastavneAktivnostiController> _logger;

        public NastavneAktivnostiController(ApplicationDbContext context, ILogger<NastavneAktivnostiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: NastavneAktivnosti/Index?predmetId=5
        public async Task<IActionResult> Index(long predmetId)
        {
            var predmet = await _context.Predmeti.FindAsync(predmetId);
            if (predmet == null) return NotFound();

            var nastavneAktivnosti = _context.NastavneAktivnosti
                .Where(n => n.PredmetId == predmetId)
                    .Include(n => n.Predmet)
                    .Include(n => n.NastavniMaterijali)
                    .Include(n => n.Komentari)
                    .Include(n => n.Ocjene)
                    .OrderBy(n => n.DatumVrijemeOdrzavanja);

            ViewBag.PredmetNaziv = predmet.Naziv;
            ViewBag.PredmetId = predmetId;
            return View(await nastavneAktivnosti.ToListAsync());
        }

        // GET: NastavneAktivnosti/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var nastavnaAktivnost = await _context.NastavneAktivnosti
                .Include(n => n.Predmet)
                    .Include(n => n.NastavniMaterijali)
                    .Include(n => n.Komentari)
                        .ThenInclude(k => k.Student)
                    .Include(n => n.Ocjene)
                        .ThenInclude(o => o.Student)
                    .FirstOrDefaultAsync(n => n.Id == id);

            if (nastavnaAktivnost == null) return NotFound();

            // Provjera dostupnosti za studente
            if (!nastavnaAktivnost.JeDostupno && User.IsInRole("Student"))
            {
                TempData["Error"] = "Ova aktivnost još nije dostupna";
                return RedirectToAction("Details", "Predmet", new { id = nastavnaAktivnost.PredmetId });
            }

            return View(nastavnaAktivnost);
        }

        [Authorize(Roles = "Profesor,Asistent")]
        public IActionResult Create(long predmetId)
        {
            var predmet = _context.Predmeti.Find(predmetId);
            if (predmet == null) return NotFound();

            var viewModel = new NastavnaAktivnostCreateViewModel
            {
                PredmetId = predmetId,
                DatumVrijemeOdrzavanja = DateTime.Now
            };

            ViewBag.PredmetNaziv = predmet.Naziv;
            return View(viewModel);
        }

        // POST: NastavneAktivnosti/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> Create(NastavnaAktivnostCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var nastavnaAktivnost = new NastavnaAktivnost
                {
                    Naziv = viewModel.Naziv,
                    Opis = viewModel.Opis,
                    Tip = viewModel.Tip,
                    DatumVrijemeOdrzavanja = viewModel.DatumVrijemeOdrzavanja,
                    ManuelnoOtkljucano = viewModel.ManuelnoOtkljucano,
                    PredmetId = viewModel.PredmetId
                };
                _context.Add(nastavnaAktivnost);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Nastavna aktivnost uspješno kreirana.";
                return RedirectToAction(nameof(Index), new { predmetId = viewModel.PredmetId });
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            _logger.LogWarning("ModelState invalid: {Errors}", string.Join(", ", errors));
            TempData["Error"] = "Greška pri kreiranju: " + string.Join(", ", errors);

            ViewBag.PredmetNaziv = _context.Predmeti.Find(viewModel.PredmetId)?.Naziv;
            return View(viewModel);
        }

        // GET: NastavneAktivnosti/Edit/5
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var nastavnaAktivnost = await _context.NastavneAktivnosti
                .Include(n => n.Predmet)
                .FirstOrDefaultAsync(n => n.Id == id);
            if (nastavnaAktivnost == null) return NotFound();

            ViewBag.PredmetId = nastavnaAktivnost.PredmetId;
            ViewBag.PredmetNaziv = nastavnaAktivnost.Predmet.Naziv;
            return View(nastavnaAktivnost);
        }

        // POST: NastavneAktivnosti/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Naziv,Opis,Tip,DatumVrijemeOdrzavanja,ManuelnoOtkljucano,PredmetId")] NastavnaAktivnost nastavnaAktivnost)
        {
            if (id != nastavnaAktivnost.Id)
            {
                TempData["Error"] = "Neispravan ID aktivnosti";
                return RedirectToAction(nameof(Index));
            }

            var originalAktivnost = await _context.NastavneAktivnosti
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == id);

            if (originalAktivnost == null) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nastavnaAktivnost);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Promjene uspješno spremljene";
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Konfliktn ažuriranje aktivnosti ID: {Id}", id);
                    TempData["Error"] = "Došlo je do konflikta prilikom ažuriranja";
                }
                return RedirectToAction(nameof(Index), new { predmetId = nastavnaAktivnost.PredmetId });
            }

            ViewBag.PredmetId = nastavnaAktivnost.PredmetId;
            ViewBag.PredmetNaziv = _context.Predmeti.Find(nastavnaAktivnost.PredmetId)?.Naziv;
            return View(nastavnaAktivnost);
        }

        // GET: NastavneAktivnosti/Delete/5
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null) return NotFound();

            var nastavnaAktivnost = await _context.NastavneAktivnosti
                .Include(n => n.Predmet)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (nastavnaAktivnost == null) return NotFound();

            return View(nastavnaAktivnost);
        }

        // POST: NastavneAktivnosti/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var nastavnaAktivnost = await _context.NastavneAktivnosti
                .Include(n => n.NastavniMaterijali)
                .Include(n => n.Komentari)
                .Include(n => n.Ocjene)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (nastavnaAktivnost == null)
            {
                TempData["Error"] = "Aktivnost nije pronađena";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Brišemo sve povezane resurse
                _context.RemoveRange(nastavnaAktivnost.NastavniMaterijali);
                _context.RemoveRange(nastavnaAktivnost.Komentari);
                _context.RemoveRange(nastavnaAktivnost.Ocjene);

                _context.NastavneAktivnosti.Remove(nastavnaAktivnost);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Aktivnost i svi povezani resursi uspješno obrisani";
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Greška pri brisanju aktivnosti ID: {Id}", id);
                TempData["Error"] = "Došlo je do greške prilikom brisanja aktivnosti";
            }

            return RedirectToAction(nameof(Index), new { predmetId = nastavnaAktivnost.PredmetId });
        }

        // POST: NastavneAktivnosti/ToggleLock/5
        [HttpPost]
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> ToggleLock(long id)
        {
            var aktivnost = await _context.NastavneAktivnosti.FindAsync(id);
            if (aktivnost == null) return NotFound();

            if (aktivnost.DatumVrijemeOdrzavanja > DateTime.Now)
            {
                aktivnost.ManuelnoOtkljucano = !aktivnost.ManuelnoOtkljucano;
            }
            else
            {
                aktivnost.ManuelnoZakljucano = !aktivnost.ManuelnoZakljucano;
            }

            _context.Update(aktivnost);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        private bool NastavnaAktivnostExists(long id)
        {
            return _context.NastavneAktivnosti.Any(e => e.Id == id);
        }
    }
}