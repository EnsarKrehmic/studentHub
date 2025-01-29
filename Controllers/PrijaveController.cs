using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using System.Security.Claims;

namespace StudentHub.Controllers
{
    [Route("Prijave")]
    public class PrijaveController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PrijaveController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Prijave
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var prijave = _context.Prijave
                .Include(p => p.Ispit)
                .Include(p => p.Student);
            return View(prijave);

            var applicationDbContext = _context.Prijave
                .Include(p => p.Ispit)
                .Include(p => p.Student);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Prijave/Details/{id}
        [HttpGet("Details/{id:long}")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var prijava = await _context.Prijave
                .Include(p => p.Ispit)
                .Include(p => p.Student)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (prijava == null ||
                (User.IsInRole("Student") && prijava.StudentId.ToString() != User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                return Forbid();
            }

            return View(prijava);
        }

        // GET: Prijave/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            ViewData["IspitId"] = new SelectList(_context.Ispiti, "Id", "Naziv");
            return View();
        }

        // POST: Prijave/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DatumPrijave,IspitId")] Prijava prijava)
        {
            prijava.StudentId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Fetch the exam by IspitId
            var ispit = await _context.Ispiti
                .Include(i => i.Predmet)
                .Include(i => i.NastavniPlan)
                .Include(i => i.StudijskiProgram)
                .FirstOrDefaultAsync(i => i.Id == prijava.IspitId);

            if (ispit == null)
            {
                ModelState.AddModelError("IspitId", "Ispit ne postoji.");
                ViewData["IspitId"] = new SelectList(_context.Ispiti, "Id", "Naziv", prijava.IspitId);
                return View(prijava);
            }

            // Check if the exam date is expired
            if (ispit.DatumOdrzavanja < DateTime.Now)
            {
                ModelState.AddModelError("IspitId", "Datum ispita je istekao.");
                ViewData["IspitId"] = new SelectList(_context.Ispiti, "Id", "Naziv", prijava.IspitId);
                return View(prijava);
            }

            // Check if the registration period is valid (3 days before the exam date)
            if (ispit.DatumOdrzavanja.AddDays(-3) <= DateTime.Now)
            {
                ModelState.AddModelError("IspitId", "Rok za prijavu ispita je istekao.");
                ViewData["IspitId"] = new SelectList(_context.Ispiti, "Id", "Naziv", prijava.IspitId);
                return View(prijava);
            }

            // Check if the student is enrolled in the same study program, curriculum, and subject
            var studentId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var student = await _context.Studenti
                .Include(s => s.StudijskiProgram)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null || student.StudijskiProgramId != ispit.StudijskiProgramId)
            {
                ModelState.AddModelError("IspitId", "Niste upisani u odgovarajući studijski program.");
                ViewData["IspitId"] = new SelectList(_context.Ispiti, "Id", "Naziv", prijava.IspitId);
                return View(prijava);
            }

            if (ModelState.IsValid)
            {
                _context.Add(prijava);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["IspitId"] = new SelectList(_context.Ispiti, "Id", "Naziv", prijava.IspitId);
            return View(prijava);
        }

        // GET: Prijave/Delete/{id}
        [HttpGet("Delete/{id:long}")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null) return NotFound();

            var prijava = await _context.Prijave
                .Include(p => p.Ispit)
                .Include(p => p.Student)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (prijava == null) return NotFound();

            return View(prijava);
        }

        // POST: Prijave/Delete/{id}
        [HttpPost("Delete/{id:long}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var prijava = await _context.Prijave.FindAsync(id);
            if (prijava != null) _context.Prijave.Remove(prijava);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PrijavaExists(long id)
        {
            return _context.Prijave.Any(e => e.Id == id);
        }
    }
}