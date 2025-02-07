using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StudentHub.Controllers
{
    [Route("Dokumenti")]
    [Authorize]
    public class DokumentiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public DokumentiController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Dokumenti
        [HttpGet("")]
        [Authorize(Roles = "Student, Studentska služba")]
        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["IndexSortParm"] = sortOrder == "index_asc" ? "index_desc" : "index_asc";
            ViewData["CurrentFilter"] = searchString;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var dokumentiQuery = _context.Dokumenti
                .Include(d => d.Student)
                .ThenInclude(s => s.StudentStudijskiProgrami)
                .ThenInclude(ssp => ssp.StudijskiProgram)
                .AsQueryable();

            // Filtriranje samo za studente
            if (User.IsInRole("Student"))
            {
                dokumentiQuery = dokumentiQuery.Where(d => d.Student.AspNetUserId == userId);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                dokumentiQuery = dokumentiQuery.Where(d => d.Student.Ime.Contains(searchString) || d.Student.Prezime.Contains(searchString));
            }

            var dokumenti = await dokumentiQuery.ToListAsync();

            switch (sortOrder)
            {
                case "name_desc":
                    dokumenti = dokumenti.OrderByDescending(d => d.Student.Ime).ToList();
                    break;
                case "index_asc":
                    dokumenti = dokumenti.OrderBy(d => d.Student.BrojIndeksa).ToList();
                    break;
                case "index_desc":
                    dokumenti = dokumenti.OrderByDescending(d => d.Student.BrojIndeksa).ToList();
                    break;
                default:
                    dokumenti = dokumenti.OrderBy(d => d.Student.Ime).ToList();
                    break;
            }

            var groupedDokumenti = dokumenti
                .GroupBy(d => d.Student.StudentStudijskiProgrami.FirstOrDefault()?.StudijskiProgram)
                .Select(g => new DokumentGroupedByProgramViewModel
                {
                    StudijskiProgram = g.Key,
                    Dokumenti = g.ToList()
                })
                .ToList();

            return View(groupedDokumenti);
        }

        // GET: Dokumenti/Details/{id}
        [HttpGet("Details/{id:long}")]
        [Authorize(Roles = "Student, Studentska služba")]
        public async Task<IActionResult> Details(long id)
        {
            var dokument = await _context.Dokumenti
                .Include(d => d.StudentskaSluzba)
                .Include(d => d.Student)
                .ThenInclude(s => s.StudentStudijskiProgrami)
                .ThenInclude(ssp => ssp.StudijskiProgram)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dokument == null)
            {
                return NotFound();
            }

            // Provjera da li je prijavljeni korisnik Student
            if (User.IsInRole("Student"))
            {
                // Dohvaćanje AspNetUserId prijavljenog korisnika
                var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Provjera da li je prijavljeni student vlasnik dokumenta
                if (dokument.Student.AspNetUserId != loggedInUserId)
                {
                    return Forbid();
                }
            }

            return View(dokument);
        }

        // GET: Dokumenti/Create
        [HttpGet("Create")]
        [Authorize(Roles = "Studentska služba")]
        public IActionResult Create()
        {
            ViewBag.Studenti = new SelectList(_context.Studenti.Select(s => new { Id = s.Id, ImePrezime = s.Ime + " " + s.Prezime }), "Id", "ImePrezime");
            ViewBag.StudentskeSluzbe = new SelectList(_context.Korisnici.Where(k => k.Uloga == Uloga.StudentskaSluzba).Select(k => new { Id = k.Id, ImePrezime = k.Ime + " " + k.Prezime }), "Id", "ImePrezime");
            return View();
        }

        // POST: Dokumenti/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Create(DokumentCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = null;
                if (model.Datoteka != null)
                {
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Datoteka.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    Directory.CreateDirectory(uploadsFolder);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Datoteka.CopyToAsync(fileStream);
                    }
                }

                var dokument = new Dokument
                {
                    Naziv = model.Naziv,
                    Putanja = uniqueFileName,
                    StudentId = model.StudentId,
                    StudentskaSluzbaId = model.StudentskaSluzbaId
                };

                _context.Dokumenti.Add(dokument);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Studenti = new SelectList(_context.Studenti.Select(s => new { Id = s.Id, ImePrezime = s.Ime + " " + s.Prezime }), "Id", "ImePrezime");
            ViewBag.StudentskeSluzbe = new SelectList(_context.Korisnici.Where(k => k.Uloga == Uloga.StudentskaSluzba).Select(k => new { Id = k.Id, ImePrezime = k.Ime + " " + k.Prezime }), "Id", "ImePrezime");
            return View(model);
        }

        // GET: Dokumenti/Edit/{id}
        [HttpGet("Edit/{id:long}")]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Edit(long id)
        {
            var dokument = await _context.Dokumenti.FindAsync(id);
            if (dokument == null)
            {
                return NotFound();
            }
            var model = new DokumentEditViewModel
            {
                Id = dokument.Id,
                Naziv = dokument.Naziv,
                StudentId = dokument.StudentId
            };
            ViewBag.Studenti = new SelectList(_context.Studenti.Select(s => new { Id = s.Id, ImePrezime = s.Ime + " " + s.Prezime }), "Id", "ImePrezime");
            ViewBag.StudentskeSluzbe = new SelectList(_context.Korisnici.Where(k => k.Uloga == Uloga.StudentskaSluzba).Select(k => new { Id = k.Id, ImePrezime = k.Ime + " " + k.Prezime }), "Id", "ImePrezime");
            return View(model);
        }

        // POST: Dokumenti/Edit/{id}
        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Edit(long id, DokumentEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var dokument = await _context.Dokumenti.FindAsync(id);
                if (dokument == null)
                {
                    return NotFound();
                }

                dokument.Naziv = model.Naziv;
                dokument.StudentId = model.StudentId;

                if (model.Datoteka != null)
                {
                    // Delete the existing file
                    if (!string.IsNullOrEmpty(dokument.Putanja))
                    {
                        string existingFilePath = Path.Combine(_environment.WebRootPath, "uploads", dokument.Putanja);
                        if (System.IO.File.Exists(existingFilePath))
                        {
                            System.IO.File.Delete(existingFilePath);
                        }
                    }

                    // Upload the new file
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Datoteka.FileName;
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    Directory.CreateDirectory(uploadsFolder);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Datoteka.CopyToAsync(fileStream);
                    }
                    dokument.Putanja = uniqueFileName;
                }

                try
                {
                    _context.Update(dokument);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DokumentExists(dokument.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Studenti = new SelectList(_context.Studenti.Select(s => new { Id = s.Id, ImePrezime = s.Ime + " " + s.Prezime }), "Id", "ImePrezime");
            ViewBag.StudentskeSluzbe = new SelectList(_context.Korisnici.Where(k => k.Uloga == Uloga.StudentskaSluzba).Select(k => new { Id = k.Id, ImePrezime = k.Ime + " " + k.Prezime }), "Id", "ImePrezime");
            return View(model);
        }

        // GET: Dokumenti/Delete/{id}
        [HttpGet("Delete/{id:long}")]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Delete(long id)
        {
            var dokument = await _context.Dokumenti
                .Include(d => d.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dokument == null)
            {
                return NotFound();
            }

            return View(dokument);
        }

        // POST: Dokumenti/Delete/{id}
        [HttpPost("Delete/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var dokument = await _context.Dokumenti.FindAsync(id);
            if (dokument != null)
            {
                // Delete the file from the server
                if (!string.IsNullOrEmpty(dokument.Putanja))
                {
                    string filePath = Path.Combine(_environment.WebRootPath, "uploads", dokument.Putanja);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Dokumenti.Remove(dokument);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("GroupedByProgram")]
        [Authorize(Roles = "Student, Studentska služba")]
        public async Task<IActionResult> GroupedByProgram(string sortOrder, string searchString)
        {
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["IndexSortParm"] = sortOrder == "index_asc" ? "index_desc" : "index_asc";
            ViewData["CurrentFilter"] = searchString;

            var dokumentiQuery = _context.Dokumenti
                .Include(d => d.Student)
                .ThenInclude(s => s.StudentStudijskiProgrami)
                .ThenInclude(ssp => ssp.StudijskiProgram)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                dokumentiQuery = dokumentiQuery.Where(d => d.Student.Ime.Contains(searchString) || d.Student.Prezime.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    dokumentiQuery = dokumentiQuery.OrderByDescending(d => d.Student.Ime);
                    break;
                case "index_asc":
                    dokumentiQuery = dokumentiQuery.OrderBy(d => d.Student.BrojIndeksa);
                    break;
                case "index_desc":
                    dokumentiQuery = dokumentiQuery.OrderByDescending(d => d.Student.BrojIndeksa);
                    break;
                default:
                    dokumentiQuery = dokumentiQuery.OrderBy(d => d.Student.Ime);
                    break;
            }

            var groupedDokumenti = await dokumentiQuery
                .GroupBy(d => d.Student.StudentStudijskiProgrami.FirstOrDefault().StudijskiProgram)
                .Select(g => new DokumentGroupedByProgramViewModel
                {
                    StudijskiProgram = g.Key,
                    Dokumenti = g.ToList()
                })
                .ToListAsync();

            return View(groupedDokumenti);
        }

        private bool DokumentExists(long id)
        {
            return _context.Dokumenti.Any(e => e.Id == id);
        }
    }
}
