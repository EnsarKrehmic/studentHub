using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudentHub.Controllers
{
    [Authorize]
    public class NastavniMaterijaliController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public NastavniMaterijaliController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment; // Za upload fajlova
        }

        // GET: NastavniMaterijali/Index?nastavnaAktivnostId=5
        public async Task<IActionResult> Index(long nastavnaAktivnostId)
        {
            var nastavnaAktivnost = await _context.NastavneAktivnosti
                .Include(n => n.Predmet)
                .FirstOrDefaultAsync(n => n.Id == nastavnaAktivnostId);
            if (nastavnaAktivnost == null) return NotFound();

            // Provjera dostupnosti za studente
            if (!nastavnaAktivnost.JeDostupno && !User.IsInRole("Profesor") && !User.IsInRole("Asistent"))
            {
                TempData["Error"] = "Nastavna aktivnost nije dostupna.";
                return RedirectToAction("Index", "NastavneAktivnosti", new { predmetId = nastavnaAktivnost.PredmetId });
            }

            var materijali = _context.NastavniMaterijali
                .Where(m => m.NastavnaAktivnostId == nastavnaAktivnostId)
                .Include(m => m.NastavnaAktivnost);

            ViewBag.NastavnaAktivnostId = nastavnaAktivnostId;
            ViewBag.NastavnaAktivnostNaziv = nastavnaAktivnost.Naziv;
            return View(await materijali.ToListAsync());
        }

        // GET: NastavniMaterijali/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var nastavniMaterijal = await _context.NastavniMaterijali
                .Include(m => m.NastavnaAktivnost).ThenInclude(n => n.Predmet)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (nastavniMaterijal == null) return NotFound();

            // Provjera dostupnosti za studente
            if (!nastavniMaterijal.NastavnaAktivnost.JeDostupno && !User.IsInRole("Profesor") && !User.IsInRole("Asistent"))
            {
                TempData["Error"] = "Nastavna aktivnost nije dostupna.";
                return RedirectToAction("Index", "NastavneAktivnosti", new { predmetId = nastavniMaterijal.NastavnaAktivnost.PredmetId });
            }

            return View(nastavniMaterijal);
        }

        // GET: NastavniMaterijali/Create?nastavnaAktivnostId=5
        [Authorize(Roles = "Profesor,Asistent")]
        public IActionResult Create(long nastavnaAktivnostId)
        {
            var nastavnaAktivnost = _context.NastavneAktivnosti.Find(nastavnaAktivnostId);
            if (nastavnaAktivnost == null) return NotFound();

            ViewBag.NastavnaAktivnostId = nastavnaAktivnostId;
            ViewBag.NastavnaAktivnostNaziv = nastavnaAktivnost.Naziv;
            return View(new NastavniMaterijal { NastavnaAktivnostId = nastavnaAktivnostId });
        }

        // POST: NastavniMaterijali/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> Create([Bind("Naziv,Opis,NastavnaAktivnostId")] NastavniMaterijal nastavniMaterijal, IFormFile fajl)
        {
            if (ModelState.IsValid)
            {
                if (fajl != null && fajl.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads/nastavni-materijali");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + fajl.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await fajl.CopyToAsync(fileStream);
                    }
                    nastavniMaterijal.PutanjaDoFajla = "/uploads/nastavni-materijali/" + uniqueFileName;
                }

                _context.Add(nastavniMaterijal);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "NastavneAktivnosti", new { id = nastavniMaterijal.NastavnaAktivnostId });
            }

            ViewBag.NastavnaAktivnostId = nastavniMaterijal.NastavnaAktivnostId;
            ViewBag.NastavnaAktivnostNaziv = _context.NastavneAktivnosti.Find(nastavniMaterijal.NastavnaAktivnostId)?.Naziv;
            return View(nastavniMaterijal);
        }

        // GET: NastavniMaterijali/Edit/5
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var nastavniMaterijal = await _context.NastavniMaterijali
                .Include(m => m.NastavnaAktivnost)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (nastavniMaterijal == null) return NotFound();

            ViewBag.NastavnaAktivnostId = nastavniMaterijal.NastavnaAktivnostId;
            ViewBag.NastavnaAktivnostNaziv = nastavniMaterijal.NastavnaAktivnost.Naziv;
            return View(nastavniMaterijal);
        }

        // POST: NastavniMaterijali/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Naziv,Opis,NastavnaAktivnostId,PutanjaDoFajla")] NastavniMaterijal nastavniMaterijal, IFormFile? fajl)
        {
            if (id != nastavniMaterijal.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (fajl != null && fajl.Length > 0)
                    {
                        // Brisanje starog fajla ako postoji
                        if (!string.IsNullOrEmpty(nastavniMaterijal.PutanjaDoFajla))
                        {
                            var oldFilePath = Path.Combine(_environment.WebRootPath, nastavniMaterijal.PutanjaDoFajla.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                        }

                        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads/nastavni-materijali");
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + fajl.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await fajl.CopyToAsync(fileStream);
                        }
                        nastavniMaterijal.PutanjaDoFajla = "/uploads/nastavni-materijali/" + uniqueFileName;
                    }

                    _context.Update(nastavniMaterijal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NastavniMaterijalExists(nastavniMaterijal.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction("Details", "NastavneAktivnosti", new { id = nastavniMaterijal.NastavnaAktivnostId });
            }

            ViewBag.NastavnaAktivnostId = nastavniMaterijal.NastavnaAktivnostId;
            ViewBag.NastavnaAktivnostNaziv = _context.NastavneAktivnosti.Find(nastavniMaterijal.NastavnaAktivnostId)?.Naziv;
            return View(nastavniMaterijal);
        }

        // GET: NastavniMaterijali/Delete/5
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null) return NotFound();

            var nastavniMaterijal = await _context.NastavniMaterijali
                .Include(m => m.NastavnaAktivnost)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (nastavniMaterijal == null) return NotFound();

            return View(nastavniMaterijal);
        }

        // POST: NastavniMaterijali/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var nastavniMaterijal = await _context.NastavniMaterijali.FindAsync(id);
            if (nastavniMaterijal == null) return NotFound();

            // Brisanje fajla s diska
            if (!string.IsNullOrEmpty(nastavniMaterijal.PutanjaDoFajla))
            {
                var filePath = Path.Combine(_environment.WebRootPath, nastavniMaterijal.PutanjaDoFajla.TrimStart('/'));
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            }

            _context.NastavniMaterijali.Remove(nastavniMaterijal);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "NastavneAktivnosti", new { id = nastavniMaterijal.NastavnaAktivnostId });
        }

        private bool NastavniMaterijalExists(long id)
        {
            return _context.NastavniMaterijali.Any(e => e.Id == id);
        }
    }
}