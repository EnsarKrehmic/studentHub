using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StudentHub.Controllers
{
    [Authorize]
    public class NastavniMaterijaliController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<NastavniMaterijaliController> _logger;

        public NastavniMaterijaliController(ApplicationDbContext context, IWebHostEnvironment environment, ILogger<NastavniMaterijaliController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        // GET: NastavniMaterijali/Index?nastavnaAktivnostId=5
        public async Task<IActionResult> Index(long nastavnaAktivnostId)
        {
            var nastavnaAktivnost = await _context.NastavneAktivnosti
                .Include(n => n.Predmet)
                .FirstOrDefaultAsync(n => n.Id == nastavnaAktivnostId);
            if (nastavnaAktivnost == null) return NotFound();

            if (!nastavnaAktivnost.JeDostupno && !User.IsInRole("Profesor") && !User.IsInRole("Asistent"))
            {
                TempData["Error"] = "Nastavna aktivnost nije dostupna.";
                return RedirectToAction("Index", "NastavneAktivnosti", new { predmetId = nastavnaAktivnost.PredmetId });
            }

            var materijali = await _context.NastavniMaterijali
                .Where(m => m.NastavnaAktivnostId == nastavnaAktivnostId)
                .Include(m => m.Fajlovi)
                .ToListAsync();

            ViewBag.NastavnaAktivnostId = nastavnaAktivnostId;
            ViewBag.NastavnaAktivnostNaziv = nastavnaAktivnost.Naziv;
            return View(materijali);
        }

        // GET: NastavniMaterijali/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var nastavniMaterijal = await _context.NastavniMaterijali
                .Include(m => m.NastavnaAktivnost).ThenInclude(n => n.Predmet)
                .Include(m => m.Fajlovi)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (nastavniMaterijal == null) return NotFound();

            if (!nastavniMaterijal.NastavnaAktivnost.JeDostupno && !User.IsInRole("Profesor") && !User.IsInRole("Asistent"))
            {
                TempData["Error"] = "Nastavna aktivnost nije dostupna.";
                return RedirectToAction("Index", "NastavniMaterijali", new { nastavnaAktivnostId = nastavniMaterijal.NastavnaAktivnostId });
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> Create([Bind("Naziv,Opis,NastavnaAktivnostId")] NastavniMaterijal nastavniMaterijal, List<IFormFile> fajlovi)
        {
            if (fajlovi == null || !fajlovi.Any(f => f.Length > 0))
                ModelState.AddModelError("fajlovi", "Morate izabrati barem jedan fajl.");

            ModelState.Remove("NastavnaAktivnost");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Greška prilikom dodavanja materijala.";
                ViewBag.NastavnaAktivnostId = nastavniMaterijal.NastavnaAktivnostId;
                ViewBag.NastavnaAktivnostNaziv = _context.NastavneAktivnosti.Find(nastavniMaterijal.NastavnaAktivnostId)?.Naziv;
                return View(nastavniMaterijal);
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads/nastavni-materijali");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            foreach (var fajl in fajlovi.Where(f => f != null && f.Length > 0))
            {
                var uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(fajl.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await fajl.CopyToAsync(stream);

                nastavniMaterijal.Fajlovi.Add(new NastavniMaterijalFajl
                {
                    PutanjaDoFajla = "/uploads/nastavni-materijali/" + uniqueFileName,
                    TipFajla = Path.GetExtension(fajl.FileName).ToLower()
                });
            }

            _context.NastavniMaterijali.Add(nastavniMaterijal);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Materijal uspješno dodat.";
            return RedirectToAction("Index", "NastavniMaterijali", new { nastavnaAktivnostId = nastavniMaterijal.NastavnaAktivnostId });
        }

        // GET: NastavniMaterijali/Edit/5
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var nastavniMaterijal = await _context.NastavniMaterijali
                .Include(m => m.NastavnaAktivnost)
                .Include(m => m.Fajlovi)
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
        public async Task<IActionResult> Edit(long id, [Bind("Id,Naziv,Opis,NastavnaAktivnostId")] NastavniMaterijal nastavniMaterijal, List<IFormFile> fajlovi)
        {
            var existingMaterijal = await _context.NastavniMaterijali
                .Include(m => m.NastavnaAktivnost)
                .Include(m => m.Fajlovi)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (existingMaterijal == null || id != existingMaterijal.Id)
                return NotFound();

            if (!await JeAutoriziranZaPredmet(existingMaterijal.NastavnaAktivnost.PredmetId))
                return Forbid();

            existingMaterijal.Naziv = nastavniMaterijal.Naziv;
            existingMaterijal.Opis = nastavniMaterijal.Opis;

            ModelState.Remove("Fajlovi");
            ModelState.Remove("NastavnaAktivnost");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Greška prilikom uređivanja materijala.";
                ViewBag.NastavnaAktivnostId = existingMaterijal.NastavnaAktivnostId;
                ViewBag.NastavnaAktivnostNaziv = existingMaterijal.NastavnaAktivnost?.Naziv;
                return View(existingMaterijal);
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads/nastavni-materijali");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            foreach (var fajl in fajlovi.Where(f => f != null && f.Length > 0))
            {
                var uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(fajl.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await fajl.CopyToAsync(stream);

                existingMaterijal.Fajlovi.Add(new NastavniMaterijalFajl
                {
                    PutanjaDoFajla = "/uploads/nastavni-materijali/" + uniqueFileName,
                    TipFajla = Path.GetExtension(fajl.FileName).ToLower()
                });
            }

            _context.Update(existingMaterijal);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Materijal uspješno ažuriran.";
            return RedirectToAction("Index", new { nastavnaAktivnostId = existingMaterijal.NastavnaAktivnostId });
        }
        // GET: NastavniMaterijali/Delete/5
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null) return NotFound();

            var nastavniMaterijal = await _context.NastavniMaterijali
                .Include(m => m.NastavnaAktivnost)
                    .ThenInclude(na => na.Predmet)
                .Include(m => m.Fajlovi)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (nastavniMaterijal == null) return NotFound();

            if (!await JeAutoriziranZaPredmet(nastavniMaterijal.NastavnaAktivnost.PredmetId))
                return Forbid();

            return View(nastavniMaterijal);
        }

        // POST: NastavniMaterijali/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var nastavniMaterijal = await _context.NastavniMaterijali
                .Include(m => m.Fajlovi)
                .Include(m => m.NastavnaAktivnost)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (nastavniMaterijal == null) return NotFound();

            if (!await JeAutoriziranZaPredmet(nastavniMaterijal.NastavnaAktivnost.PredmetId))
                return Forbid();

            foreach (var fajl in nastavniMaterijal.Fajlovi)
            {
                if (!string.IsNullOrEmpty(fajl.PutanjaDoFajla))
                {
                    var fullPath = Path.Combine(_environment.WebRootPath, fajl.PutanjaDoFajla.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
                }
            }

            _context.NastavniMaterijalFajlovi.RemoveRange(nastavniMaterijal.Fajlovi);
            _context.NastavniMaterijali.Remove(nastavniMaterijal);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "NastavniMaterijali", new { nastavnaAktivnostId = nastavniMaterijal.NastavnaAktivnostId });
        }

        // POST: NastavniMaterijali/ObrisiFajl
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Profesor,Asistent")]
        public async Task<IActionResult> ObrisiFajl(long id)
        {
            var fajl = await _context.NastavniMaterijalFajlovi
                .Include(f => f.NastavniMaterijal)
                .ThenInclude(nm => nm.NastavnaAktivnost)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (fajl == null)
                return NotFound();

            if (!await JeAutoriziranZaPredmet(fajl.NastavniMaterijal.NastavnaAktivnost.PredmetId))
                return Forbid();

            var filePath = Path.Combine(_environment.WebRootPath, fajl.PutanjaDoFajla.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.NastavniMaterijalFajlovi.Remove(fajl);
            await _context.SaveChangesAsync();

            // Ako je zahtjev došao kao fetch/AJAX, vrati JSON bez redirekcije
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Ok(new { success = true });
            }

            // U suprotnom, fallback redirekcija na Edit
            TempData["Success"] = "Fajl je obrisan.";
            return RedirectToAction("Edit", new { id = fajl.NastavniMaterijalId });
        }

        // GET: NastavniMaterijali/Download/5
        public async Task<IActionResult> Download(long? id)
        {
            if (id == null) return NotFound();

            var fajl = await _context.NastavniMaterijalFajlovi
                .Include(f => f.NastavniMaterijal)
                .ThenInclude(m => m.NastavnaAktivnost)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (fajl == null || string.IsNullOrEmpty(fajl.PutanjaDoFajla)) return NotFound();

            if (!fajl.NastavniMaterijal.NastavnaAktivnost.JeDostupno &&
                !User.IsInRole("Profesor") &&
                !User.IsInRole("Asistent"))
            {
                return Forbid();
            }

            var filePath = Path.Combine(_environment.WebRootPath, fajl.PutanjaDoFajla.TrimStart('/'));
            if (!System.IO.File.Exists(filePath)) return NotFound();

            var contentType = GetContentType(fajl.TipFajla) ?? "application/octet-stream";
            var safeFileName = Path.GetFileName(filePath);

            return PhysicalFile(filePath, contentType, safeFileName);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadAll(long id)
        {
            var materijal = await _context.NastavniMaterijali
                .Include(m => m.Fajlovi)
                .Include(m => m.NastavnaAktivnost)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (materijal == null || !materijal.Fajlovi.Any())
                return NotFound();

            // Provjera dostupnosti studentima
            if (!materijal.NastavnaAktivnost.JeDostupno &&
                !User.IsInRole("Profesor") &&
                !User.IsInRole("Asistent"))
            {
                return Forbid();
            }

            // Kreiranje ZIP fajla u memoriji
            var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var fajl in materijal.Fajlovi)
                {
                    var filePath = Path.Combine(_environment.WebRootPath, fajl.PutanjaDoFajla.TrimStart('/'));

                    if (System.IO.File.Exists(filePath))
                    {
                        var entryName = Path.GetFileName(fajl.PutanjaDoFajla);
                        var entry = archive.CreateEntry(entryName);

                        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                        using var entryStream = entry.Open();
                        await fileStream.CopyToAsync(entryStream);
                    }
                }
            }

            memoryStream.Position = 0;

            var zipFileName = $"Materijali_{materijal.Naziv}.zip";
            return File(memoryStream, "application/zip", zipFileName);
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
