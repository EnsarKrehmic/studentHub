using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

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
            var nastavnaAktivnost = _context.NastavneAktivnosti
                .Include(na => na.Predmet)
                    .ThenInclude(p => p.Profesor)
                .Include(na => na.Predmet)
                    .ThenInclude(p => p.Asistent)
                .FirstOrDefault(na => na.Id == nastavnaAktivnostId);

            if (nastavnaAktivnost?.Predmet == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            bool isAuthorized = (User.IsInRole("Profesor") &&
                                nastavnaAktivnost.Predmet.Profesor?.AspNetUserId == userId) ||
                               (User.IsInRole("Asistent") &&
                                nastavnaAktivnost.Predmet.Asistent?.AspNetUserId == userId);

            if (!isAuthorized) return Forbid();

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
            if (fajl == null || fajl.Length == 0)
            {
                ModelState.AddModelError("fajl", "Molimo izaberite fajl.");
            }

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
                    nastavniMaterijal.TipFajla = Path.GetExtension(fajl.FileName).ToLower(); // Set file type
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
                .ThenInclude(na => na.Predmet)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (nastavniMaterijal == null) return NotFound();

            if (!await JeAutoriziranZaPredmet(nastavniMaterijal.NastavnaAktivnost.PredmetId))
                return Forbid();

            ViewBag.NastavnaAktivnostId = nastavniMaterijal.NastavnaAktivnostId;
            ViewBag.NastavnaAktivnostNaziv = nastavniMaterijal.NastavnaAktivnost.Naziv;
            return View(nastavniMaterijal);
        }

        // POST: NastavniMaterijali/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Naziv,Opis,NastavnaAktivnostId")] NastavniMaterijal nastavniMaterijal, IFormFile fajl)
        {
            var existingMaterijal = await _context.NastavniMaterijali.FindAsync(id);
            if (existingMaterijal == null) return NotFound();

            if (!await JeAutoriziranZaPredmet(existingMaterijal.NastavnaAktivnost.PredmetId))
                return Forbid();

            if (id != existingMaterijal.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Ažuriraj samo dozvoljena polja
                    existingMaterijal.Naziv = nastavniMaterijal.Naziv;
                    existingMaterijal.Opis = nastavniMaterijal.Opis;

                    if (fajl != null && fajl.Length > 0)
                    {
                        // Brisanje starog fajla
                        if (!string.IsNullOrEmpty(existingMaterijal.PutanjaDoFajla))
                        {
                            var oldFilePath = Path.Combine(_environment.WebRootPath, existingMaterijal.PutanjaDoFajla.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                        }

                        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads/nastavni-materijali");
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + fajl.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await fajl.CopyToAsync(fileStream);
                        }
                        existingMaterijal.PutanjaDoFajla = "/uploads/nastavni-materijali/" + uniqueFileName;
                        existingMaterijal.TipFajla = Path.GetExtension(fajl.FileName).ToLower();
                    }

                    _context.Update(existingMaterijal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NastavniMaterijalExists(existingMaterijal.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction("Details", "NastavneAktivnosti", new { id = existingMaterijal.NastavnaAktivnostId });
            }
            return View(existingMaterijal);
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
            var nastavniMaterijal = await _context.NastavniMaterijali
                .Include(m => m.NastavnaAktivnost)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (nastavniMaterijal == null) return NotFound();

            if (!await JeAutoriziranZaPredmet(nastavniMaterijal.NastavnaAktivnost.PredmetId))
                return Forbid();

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

        public async Task<IActionResult> Download(long? id)
        {
            if (id == null) return NotFound();

            var materijal = await _context.NastavniMaterijali
                .Include(m => m.NastavnaAktivnost)
                .ThenInclude(na => na.Predmet)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (materijal == null || string.IsNullOrEmpty(materijal.PutanjaDoFajla))
                return NotFound();

            // Provjera autorizacije
            if (!materijal.NastavnaAktivnost.JeDostupno &&
                !User.IsInRole("Profesor") &&
                !User.IsInRole("Asistent"))
            {
                return Forbid();
            }

            var filePath = Path.Combine(_environment.WebRootPath, materijal.PutanjaDoFajla.TrimStart('/'));
            if (!System.IO.File.Exists(filePath)) return NotFound();

            return PhysicalFile(filePath, GetContentType(materijal.TipFajla), $"{materijal.Naziv}{materijal.TipFajla}");
        }

        private string GetContentType(string fileExtension)
        {
            return fileExtension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".txt" => "text/plain",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream",
            };
        }

        private async Task<bool> JeAutoriziranZaPredmet(long predmetId)
        {
            var predmet = await _context.Predmeti
                .Include(p => p.Profesor)
                .Include(p => p.Asistent)
                .FirstOrDefaultAsync(p => p.Id == predmetId);

            if (predmet == null) return false;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return (predmet.Profesor != null && predmet.Profesor.AspNetUserId == userId) ||
                   (predmet.Asistent != null && predmet.Asistent.AspNetUserId == userId);
        }

        private bool NastavniMaterijalExists(long id)
        {
            return _context.NastavniMaterijali.Any(e => e.Id == id);
        }
    }
}
