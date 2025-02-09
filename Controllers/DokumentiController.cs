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
        public async Task<IActionResult> Index(string sortOrder, string searchString, int? studijskiProgramId)
        {
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["IndexSortParm"] = sortOrder == "index_asc" ? "index_desc" : "index_asc";
            ViewData["CurrentFilter"] = searchString;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var dokumentiQuery = _context.Dokumenti
                .Include(d => d.Slike)
                .Include(d => d.Student)
                .ThenInclude(s => s.StudentStudijskiProgrami)
                .ThenInclude(ssp => ssp.StudijskiProgram)
                .AsQueryable();

            // Filtriranje samo za studente
            if (User.IsInRole("Student"))
            {
                dokumentiQuery = dokumentiQuery.Where(d => d.Student.AspNetUserId == userId);
            }

            // Filtriranje po imenu/prezimenu studenta
            if (!string.IsNullOrEmpty(searchString))
            {
                dokumentiQuery = dokumentiQuery.Where(d =>
                    d.Student.Ime.Contains(searchString) ||
                    d.Student.Prezime.Contains(searchString));
            }

            // Filtriranje po studijskom programu ako je odabran
            if (studijskiProgramId.HasValue)
            {
                dokumentiQuery = dokumentiQuery.Where(d =>
                    d.Student.StudentStudijskiProgrami.Any(ssp => ssp.StudijskiProgramId == studijskiProgramId.Value));
            }

            var dokumenti = await dokumentiQuery.ToListAsync();

            // Sortiranje podataka
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

            // Grupisanje dokumenata po studijskom programu
            var groupedDokumenti = dokumenti
                .GroupBy(d => d.Student.StudentStudijskiProgrami.FirstOrDefault()?.StudijskiProgram)
                .Select(g => new DokumentGroupedByProgramViewModel
                {
                    StudijskiProgram = g.Key,
                    Dokumenti = g.ToList()
                })
                .ToList();

            // Prosljeđivanje liste studijskih programa za filtriranje u ViewBag
            ViewBag.StudijskiProgrami = new SelectList(await _context.StudijskiProgrami.ToListAsync(), "Id", "Naziv");

            return View(groupedDokumenti);
        }

        // GET: Dokumenti/Details/{id}
        [HttpGet("Details/{id:long}")]
        [Authorize(Roles = "Student, Studentska služba")]
        public async Task<IActionResult> Details(long id)
        {
            var dokument = await _context.Dokumenti
                .Include(d => d.Slike)
                .Include(d => d.StudentskaSluzba)
                .Include(d => d.Student)
                .ThenInclude(s => s.StudentStudijskiProgrami)
                .ThenInclude(ssp => ssp.StudijskiProgram)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (dokument == null)
            {
                return NotFound();
            }

            // Provjera da li je dokument validan i studenti imaju pravo pristupa
            if (User.IsInRole("Student"))
            {
                var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (dokument.Student == null || dokument.Student.AspNetUserId != loggedInUserId)
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
                var dokument = new Dokument
                {
                    Naziv = model.Naziv,
                    StudentId = model.StudentId,
                    StudentskaSluzbaId = model.StudentskaSluzbaId
                };

                _context.Dokumenti.Add(dokument);
                await _context.SaveChangesAsync();

                if (model.Slike != null && model.Slike.Count > 0)
                {
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploadsFolder);

                    foreach (var slika in model.Slike)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + slika.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await slika.CopyToAsync(fileStream);
                        }

                        var dokumentSlike = new DokumentSlike
                        {
                            DokumentId = dokument.Id,
                            Putanja = uniqueFileName
                        };

                        _context.DokumentSlike.Add(dokumentSlike);
                    }

                    await _context.SaveChangesAsync();
                }

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
            var dokument = await _context.Dokumenti
                .Include(d => d.Slike) // Učitavamo povezane slike
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dokument == null)
            {
                return NotFound();
            }

            var model = new DokumentEditViewModel
            {
                Id = dokument.Id,
                Naziv = dokument.Naziv,
                StudentId = dokument.StudentId,
                StudentskaSluzbaId = dokument.StudentskaSluzbaId,
                PostojeceSlike = dokument.Slike.Select(s => new DokumentSlike { Id = s.Id, Putanja = s.Putanja }).ToList()
            };

            ViewBag.Studenti = new SelectList(
                _context.Studenti.Select(s => new { Id = s.Id, ImePrezime = s.Ime + " " + s.Prezime }), "Id", "ImePrezime");

            ViewBag.StudentskeSluzbe = new SelectList(
                _context.Korisnici.Where(k => k.Uloga == Uloga.StudentskaSluzba)
                    .Select(k => new { Id = k.Id, ImePrezime = k.Ime + " " + k.Prezime }),
                "Id", "ImePrezime");

            return View(model);
        }

        [HttpPost("EditPost/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> EditPost(long id, DokumentEditViewModel model, [FromForm] List<long> ObrisiSlike)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var dokument = await _context.Dokumenti
                    .Include(d => d.Slike)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (dokument == null)
                {
                    return NotFound();
                }

                dokument.Naziv = model.Naziv;
                dokument.StudentId = model.StudentId;

                // Brisanje slika koje su označene
                if (ObrisiSlike != null && ObrisiSlike.Count > 0)
                {
                    foreach (var slikaId in ObrisiSlike)
                    {
                        var slika = dokument.Slike.FirstOrDefault(s => s.Id == slikaId);
                        if (slika != null)
                        {
                            string filePath = Path.Combine(_environment.WebRootPath, "uploads", slika.Putanja);
                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);
                            }

                            _context.DokumentSlike.Remove(slika);
                        }
                    }
                }

                // Dodavanje novih slika
                if (model.NoveSlike != null && model.NoveSlike.Count > 0)
                {
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploadsFolder);

                    foreach (var slika in model.NoveSlike)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + slika.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await slika.CopyToAsync(fileStream);
                        }

                        var dokumentSlike = new DokumentSlike
                        {
                            DokumentId = dokument.Id,
                            Putanja = uniqueFileName
                        };

                        _context.DokumentSlike.Add(dokumentSlike);
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Dokumenti/Delete/{id}
        [HttpGet("Delete/{id:long}")]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Delete(long id)
        {
            var dokument = await _context.Dokumenti
                .Include(d => d.Student)
                .Include(d => d.Slike)
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
            var dokument = await _context.Dokumenti
                .Include(d => d.Slike)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (dokument != null)
            {
                // Delete the files from the server
                foreach (var slika in dokument.Slike)
                {
                    string filePath = Path.Combine(_environment.WebRootPath, "uploads", slika.Putanja);
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
