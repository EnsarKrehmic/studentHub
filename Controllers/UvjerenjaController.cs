using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StudentHub.Controllers
{
    [Route("Uvjerenja")]
    [Authorize]
    public class UvjerenjaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UvjerenjaController> _logger;

        public UvjerenjaController(ApplicationDbContext context, ILogger<UvjerenjaController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Uvjerenja
        [HttpGet("")]
        [Authorize(Roles = "Student, Studentska služba")]
        public async Task<IActionResult> Index(string searchString, long? studijskiProgramId)
        {
            var query = _context.Uvjerenja
                .Include(u => u.Student)
                .Include(u => u.StudentskaSluzba)
                .AsQueryable();

            if (User.IsInRole("Student"))
            {
                string studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                query = query.Where(u => u.Student.AspNetUserId == studentId);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(u => u.Student.Ime.Contains(searchString) || u.Student.Prezime.Contains(searchString));
            }

            if (studijskiProgramId.HasValue)
            {
                query = query.Where(u => u.Student.StudentStudijskiProgrami.Any(s => s.StudijskiProgramId == studijskiProgramId));
            }

            var uvjerenja = await query.ToListAsync();
            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", studijskiProgramId);

            return View(uvjerenja);
        }

        // GET: Uvjerenja/Details/{id}
        [HttpGet("Details/{id:long}")]
        [Authorize(Roles = "Student, Studentska služba")]
        public async Task<IActionResult> Details(long id)
        {
            var uvjerenje = await _context.Uvjerenja
                .Include(u => u.Student)
                .Include(u => u.StudentskaSluzba)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (uvjerenje == null || (User.IsInRole("Student") && uvjerenje.Student.AspNetUserId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value))
            {
                return NotFound();
            }

            return View(uvjerenje);
        }

        // GET: Uvjerenja/Create
        [HttpGet("Create")]
        [Authorize(Roles = "Studentska služba")]
        public IActionResult Create()
        {
            ViewBag.Studenti = new SelectList(_context.Studenti
                   .Select(s => new
                   {
                       Id = s.Id,
                       Prikaz = $"{s.Ime} {s.Prezime} [{s.BrojIndeksa}]"
                   }),
                   "Id", "Prikaz");

            return View();
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Create([Bind("Namjena,StudentId,VrstaUvjerenja")] Uvjerenje uvjerenje)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var studentskasluzba = await _context.StudentskeSluzbe.FirstOrDefaultAsync(s => s.AspNetUserId == userId);

            if (studentskasluzba == null)
            {
                _logger.LogError("Nije pronađen član studentske službe sa ID: {UserId}", userId);
                return BadRequest("Ne možete kreirati uvjerenje jer niste registrovani kao član studentske službe.");
            }

            if (!Enum.IsDefined(typeof(VrstaUvjerenja), uvjerenje.VrstaUvjerenja))
            {
                ModelState.AddModelError("VrstaUvjerenja", "Neispravna vrijednost za vrstu uvjerenja.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    uvjerenje.StudentskaSluzbaId = studentskasluzba.Id;
                    uvjerenje.DatumIzdavanja = DateTime.Now;

                    uvjerenje.Student = await _context.Studenti
                        .FirstOrDefaultAsync(s => s.Id == uvjerenje.StudentId);

                    if (uvjerenje.Student == null)
                    {
                        ModelState.AddModelError("StudentId", "Odabrani student ne postoji.");
                        ViewBag.Studenti = new SelectList(_context.Studenti
                           .Select(s => new { Id = s.Id, Prikaz = $"{s.Ime} {s.Prezime} [{s.BrojIndeksa}]" })
                           .ToList(), "Id", "Prikaz");

                        return View(uvjerenje);
                    }

                    _context.Uvjerenja.Add(uvjerenje);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Uvjerenje uspješno kreirano za Student ID: {StudentId}", uvjerenje.StudentId);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Greška pri kreiranju uvjerenja.");
                    return StatusCode(500, "Internal server error");
                }
            }

            _logger.LogWarning("Neispravan ModelState: {Errors}", string.Join(", ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)));

            ViewBag.Studenti = new SelectList(_context.Studenti
               .Select(s => new { Id = s.Id, Prikaz = $"{s.Ime} {s.Prezime} [{s.BrojIndeksa}]" })
               .ToList(), "Id", "Prikaz");

            return View(uvjerenje);
        }

        [HttpGet("Edit/{id:long}")]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Edit(long id)
        {
            var uvjerenje = await _context.Uvjerenja
                .Include(u => u.Student)
                .Include(u => u.StudentskaSluzba)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (uvjerenje == null)
            {
                return NotFound();
            }

            ViewBag.Studenti = new SelectList(_context.Studenti
                .Select(s => new
                {
                    Id = s.Id,
                    Prikaz = $"{s.Ime} {s.Prezime} [{s.BrojIndeksa}]"
                }),
                "Id", "Prikaz", uvjerenje.StudentId);

            return View(uvjerenje);
        }

        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Namjena,StudentId,VrstaUvjerenja")] Uvjerenje uvjerenje)
        {
            if (id != uvjerenje.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUvjerenje = await _context.Uvjerenja
                        .Include(u => u.Student)
                        .Include(u => u.StudentskaSluzba)
                        .FirstOrDefaultAsync(u => u.Id == id);

                    if (existingUvjerenje == null)
                    {
                        return NotFound();
                    }

                    // Ažuriramo polja
                    existingUvjerenje.Namjena = uvjerenje.Namjena;
                    existingUvjerenje.StudentId = uvjerenje.StudentId;
                    existingUvjerenje.VrstaUvjerenja = uvjerenje.VrstaUvjerenja;

                    _context.Update(existingUvjerenje);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UvjerenjeExists(uvjerenje.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.Studenti = new SelectList(_context.Studenti
                .Select(s => new
                {
                    Id = s.Id,
                    Prikaz = $"{s.Ime} {s.Prezime} [{s.BrojIndeksa}]"
                }),
                "Id", "Prikaz", uvjerenje.StudentId);

            return View(uvjerenje);
        }

        // GET: Uvjerenja/Delete/{id}
        [HttpGet("Delete/{id:long}")]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Delete(long id)
        {
            var uvjerenje = await _context.Uvjerenja
                .Include(u => u.Student)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (uvjerenje == null)
            {
                return NotFound();
            }

            return View(uvjerenje);
        }

        // POST: Uvjerenja/Delete/{id}
        [HttpPost("Delete/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var uvjerenje = await _context.Uvjerenja.FindAsync(id);
            if (uvjerenje != null)
            {
                _context.Uvjerenja.Remove(uvjerenje);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool UvjerenjeExists(long id)
        {
            return _context.Uvjerenja.Any(e => e.Id == id);
        }
    }
}
