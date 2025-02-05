using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;

namespace StudentHub.Controllers
{
    [Route("Dokumenti")]
    [Authorize]
    public class DokumentiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DokumentiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Dokumenti
        [HttpGet("")]
        [Authorize(Roles = "Student, Studentska služba")]
        public async Task<IActionResult> Index()
        {
            var dokumenti = await _context.Dokumenti.Include(d => d.Student).ToListAsync();
            return View(dokumenti);
        }

        // GET: Dokumenti/Details/{id}
        [HttpGet("Details/{id:long}")]
        [Authorize(Roles = "Student, Studentska služba")]
        public async Task<IActionResult> Details(long id)
        {
            var dokument = await _context.Dokumenti
                .Include(d => d.Student)
                .Include(d => d.StudentskaSluzba)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dokument == null)
            {
                return NotFound();
            }

            return View(dokument);
        }

        // GET: Dokumenti/Create
        [HttpGet("Create")]
        [Authorize(Roles = "Studentska služba")]
        public IActionResult Create()
        {
            ViewBag.Studenti = new SelectList(_context.Studenti, "Id", "ImePrezime");
            return View();
        }

        // POST: Dokumenti/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Create([Bind("Naziv,Putanja,StudentId")] Dokument dokument)
        {
            if (ModelState.IsValid)
            {
                _context.Dokumenti.Add(dokument);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Studenti = new SelectList(_context.Studenti, "Id", "ImePrezime", dokument.StudentId);
            return View(dokument);
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
            ViewBag.Studenti = new SelectList(_context.Studenti, "Id", "ImePrezime", dokument.StudentId);
            return View(dokument);
        }

        // POST: Dokumenti/Edit/{id}
        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Naziv,Putanja,StudentId")] Dokument dokument)
        {
            if (id != dokument.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
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
            ViewBag.Studenti = new SelectList(_context.Studenti, "Id", "ImePrezime", dokument.StudentId);
            return View(dokument);
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
                _context.Dokumenti.Remove(dokument);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DokumentExists(long id)
        {
            return _context.Dokumenti.Any(e => e.Id == id);
        }
    }
}