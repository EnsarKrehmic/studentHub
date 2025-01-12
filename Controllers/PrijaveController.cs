using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using System.Security.Claims;

namespace StudentHub.Controllers
{
    [Authorize]
    [Route("Prijave")]
    public class PrijaveController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PrijaveController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Prijave
        [Authorize(Roles = "Studentska služba,Student")]
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Student"))
            {
                var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var prijave = _context.Prijave
                    .Include(p => p.Ispit)
                    .Include(p => p.Student)
                    .Where(p => p.StudentId.ToString() == studentId);
                return View(prijave);
            }

            var applicationDbContext = _context.Prijave
                .Include(p => p.Ispit)
                .Include(p => p.Student);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Prijave/Details/5
        [Authorize(Roles = "Studentska služba,Student")]
        [HttpGet]
        [Route("Details/{id}")]
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
        [Authorize(Roles = "Student")]
        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            ViewData["IspitId"] = new SelectList(_context.Ispiti, "Id", "Naziv");
            return View();
        }

        // POST: Prijave/Create
        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Create")]
        public async Task<IActionResult> Create([Bind("datumPrijave,IspitId")] Prijava prijava)
        {
            prijava.StudentId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (ModelState.IsValid)
            {
                _context.Add(prijava);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IspitId"] = new SelectList(_context.Ispiti, "Id", "Naziv", prijava.IspitId);
            return View(prijava);
        }

        // GET: Prijave/Delete/5
        [Authorize(Roles = "Studentska služba")]
        [HttpGet]
        [Route("Delete/{id}")]
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

        // POST: Prijave/Delete/5
        [Authorize(Roles = "Studentska služba")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Route("Delete/{id}")]
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
