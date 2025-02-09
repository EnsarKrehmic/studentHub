using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using System.Security.Claims;

namespace StudentHub.Controllers
{
    [Route("Zahtjevi")]
    [Authorize]
    public class ZahtjeviController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ZahtjeviController> _logger;

        public ZahtjeviController(ApplicationDbContext context, ILogger<ZahtjeviController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Zahtjevi
        [HttpGet("")]
        [Authorize(Roles = "Student, Studentska služba")]
        public async Task<IActionResult> Index(string searchString, string sortOrder, long? studijskiProgramId)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["StudijskiProgramId"] = studijskiProgramId;

            ViewData["TipSortParm"] = String.IsNullOrEmpty(sortOrder) ? "tip_desc" : "";
            ViewData["StatusSortParm"] = sortOrder == "Status" ? "status_desc" : "Status";
            ViewData["DatumSortParm"] = sortOrder == "Datum" ? "datum_desc" : "Datum";

            var zahtjevi = from z in _context.Zahtjevi.Include(z => z.Student)
                           select z;

            if (User.IsInRole("Student"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var student = await _context.Studenti.FirstOrDefaultAsync(s => s.AspNetUserId == userId);

                if (student == null)
                {
                    return BadRequest("Ne možete pregledati zahtjeve jer niste registrovani kao student.");
                }

                zahtjevi = zahtjevi.Where(z => z.StudentId == student.Id);
            }
            else
            {
                if (!String.IsNullOrEmpty(searchString))
                {
                    zahtjevi = zahtjevi.Where(z => z.Student.Ime.Contains(searchString) || z.Student.Prezime.Contains(searchString));
                }

                if (studijskiProgramId.HasValue)
                {
                    zahtjevi = zahtjevi.Where(z => z.Student.StudentStudijskiProgrami.Any(s => s.StudijskiProgramId == studijskiProgramId));
                }
            }

            switch (sortOrder)
            {
                case "tip_desc":
                    zahtjevi = zahtjevi.OrderByDescending(z => z.TipZahtjeva);
                    break;
                case "Status":
                    zahtjevi = zahtjevi.OrderBy(z => z.StatusZahtjeva);
                    break;
                case "status_desc":
                    zahtjevi = zahtjevi.OrderByDescending(z => z.StatusZahtjeva);
                    break;
                case "Datum":
                    zahtjevi = zahtjevi.OrderBy(z => z.DatumPodnosenja);
                    break;
                case "datum_desc":
                    zahtjevi = zahtjevi.OrderByDescending(z => z.DatumPodnosenja);
                    break;
                default:
                    zahtjevi = zahtjevi.OrderBy(z => z.TipZahtjeva);
                    break;
            }

            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv");

            return View(await zahtjevi.AsNoTracking().ToListAsync());
        }

        // GET: Zahtjevi/Details/{id}
        [HttpGet("Details/{id:long}")]
        [Authorize(Roles = "Student, Studentska služba")]
        public async Task<IActionResult> Details(long id)
        {
            var zahtjev = await _context.Zahtjevi
                .Include(z => z.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (zahtjev == null)
            {
                return NotFound();
            }

            return View(zahtjev);
        }

        // GET: Zahtjevi/Create
        [HttpGet("Create")]
        [Authorize(Roles = "Student")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Create([Bind("TipZahtjeva,Napomena")] Zahtjev zahtjev)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _context.Studenti.FirstOrDefaultAsync(s => s.AspNetUserId == userId);

            if (student == null)
            {
                _logger.LogError("Nije pronađen student sa ID: {UserId}", userId);
                return BadRequest("Ne možete kreirati zahtjev jer niste registrovani kao student.");
            }

            // Čišćenje ModelState vezane za Student i StudentId
            ModelState.Remove(nameof(Zahtjev.StudentId));
            ModelState.Remove(nameof(Zahtjev.Student));

            if (ModelState.IsValid)
            {
                try
                {
                    zahtjev.StudentId = student.Id;
                    zahtjev.DatumPodnosenja = DateTime.Now;
                    zahtjev.StatusZahtjeva = StatusZahtjeva.Podnešen;

                    _context.Zahtjevi.Add(zahtjev);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Zahtjev uspješno kreiran za Student ID: {StudentId}", student.Id);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Greška pri kreiranju zahtjeva za Student ID: {StudentId}", student.Id);
                    return StatusCode(500, "Internal server error");
                }
            }
            else
            {
                _logger.LogWarning("Neispravan ModelState: {ModelStateErrors}", ModelState.Values.SelectMany(v => v.Errors));
            }

            return View(zahtjev);
        }

        // GET: Zahtjevi/Edit/{id}
        [HttpGet("Edit/{id:long}")]
        [Authorize(Roles = "Student, Studentska služba")]
        public async Task<IActionResult> Edit(long id)
        {
            var zahtjev = await _context.Zahtjevi.Include(z => z.Student).FirstOrDefaultAsync(z => z.Id == id);
            if (zahtjev == null)
            {
                return NotFound("Zahtjev nije pronađen.");
            }

            if (User.IsInRole("Student") && zahtjev.StatusZahtjeva != StatusZahtjeva.Podnešen)
            {
                return Unauthorized("Nije moguće uređivati zahtjev koji nije u statusu 'Podnesen'.");
            }

            return View(zahtjev);
        }

        // POST: Zahtjevi/Edit/{id}
        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student, Studentska služba")]
        public async Task<IActionResult> Edit(long id, [Bind("Id,TipZahtjeva,StatusZahtjeva,Napomena")] Zahtjev zahtjev)
        {
            if (id != zahtjev.Id)
            {
                return NotFound("Zahtjev nije pronađen.");
            }

            var existingZahtjev = await _context.Zahtjevi.Include(z => z.Student).FirstOrDefaultAsync(z => z.Id == id);
            if (existingZahtjev == null)
            {
                return NotFound("Zahtjev nije pronađen.");
            }

            if (User.IsInRole("Student") && existingZahtjev.StatusZahtjeva != StatusZahtjeva.Podnešen)
            {
                return Unauthorized("Nije moguće uređivati zahtjev koji nije u statusu 'Podnesen'.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingZahtjev.TipZahtjeva = zahtjev.TipZahtjeva;
                    existingZahtjev.Napomena = zahtjev.Napomena;

                    if (User.IsInRole("Studentska služba"))
                    {
                        // Ažuriraj status
                        if (existingZahtjev.StatusZahtjeva != zahtjev.StatusZahtjeva)
                        {
                            existingZahtjev.StatusZahtjeva = zahtjev.StatusZahtjeva;

                            // Postavi datum rješavanja ako je zahtjev prihvaćen ili odbijen
                            if (zahtjev.StatusZahtjeva == StatusZahtjeva.Prihvaćen || zahtjev.StatusZahtjeva == StatusZahtjeva.Odbijen)
                            {
                                existingZahtjev.DatumRjesavanja = DateTime.Now;
                            }
                            else
                            {
                                existingZahtjev.DatumRjesavanja = null;
                            }
                        }
                    }

                    _context.Update(existingZahtjev);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ZahtjevExists(zahtjev.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(zahtjev);
        }

        // GET: Zahtjevi/Delete/{id}
        [HttpGet("Delete/{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var zahtjev = await _context.Zahtjevi
                .Include(z => z.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (zahtjev == null || zahtjev.StatusZahtjeva != StatusZahtjeva.Podnešen)
            {
                return NotFound();
            }

            return View(zahtjev);
        }

        // POST: Zahtjevi/Delete/{id}
        [HttpPost("Delete/{id:long}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var zahtjev = await _context.Zahtjevi.FindAsync(id);
            if (zahtjev != null && zahtjev.StatusZahtjeva == StatusZahtjeva.Podnešen)
            {
                _context.Zahtjevi.Remove(zahtjev);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(MyRequests));
        }

        // GET: Zahtjevi/MyRequests
        [HttpGet("MyRequests")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyRequests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _context.Studenti.FirstOrDefaultAsync(s => s.AspNetUserId == userId);

            if (student == null)
            {
                return BadRequest("Ne možete pregledati zahtjeve jer niste registrovani kao student.");
            }

            var zahtjevi = await _context.Zahtjevi
                .Where(z => z.StudentId == student.Id)
                .Include(z => z.Student)
                .ToListAsync();

            return View(zahtjevi);
        }
        private bool ZahtjevExists(long id)
        {
            return _context.Zahtjevi.Any(e => e.Id == id);
        }
    }
}
