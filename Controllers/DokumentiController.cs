using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;

namespace StudentHub.Controllers
{
    [Route("Dokumenti")]
    public class DokumentiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DokumentiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Dokumenti
        [HttpGet]
        [Route("")]
        [Route("[Controller]/[Action]")]
        public async Task<IActionResult> Index()
        {
            var dokumenti = _context.Dokumenti
                .Include(d => d.Student)
                .Include(d => d.StudentskaSluzba);
            return View(await dokumenti.ToListAsync());
        }

        // GET: Dokumenti/details/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var dokument = await _context.Dokumenti
                .Include(d => d.Student)
                .Include(d => d.StudentskaSluzba)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (dokument == null) return NotFound();

            return View(dokument);
        }

        // GET: Dokumenti/create
        [HttpGet]
        [Route("[Controller]/[Action]")]
        public IActionResult Create()
        {
            ViewData["brojIndeksa"] = new SelectList(_context.Studenti, "brojIndeksa", "brojIndeksa");
            ViewData["StudentskaSluzbaId"] = new SelectList(_context.StudentskeSluzbe, "Id", "Id");
            return View();
        }

        // POST: Dokumenti/create
        [HttpPost]
        [Route("[Controller]/[Action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Naziv,Putanja,brojIndeksa,StudentskaSluzbaId")] Dokument dokument)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dokument);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["brojIndeksa"] = new SelectList(_context.Studenti, "brojIndeksa", "brojIndeksa", dokument.brojIndeksa);
            ViewData["StudentskaSluzbaId"] = new SelectList(_context.StudentskeSluzbe, "Id", "Id", dokument.StudentskaSluzbaId);
            return View(dokument);
        }

        // GET: Dokumenti/edit/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var dokument = await _context.Dokumenti.FindAsync(id);
            if (dokument == null) return NotFound();

            ViewData["brojIndeksa"] = new SelectList(_context.Studenti, "brojIndeksa", "brojIndeksa", dokument.brojIndeksa);
            ViewData["StudentskaSluzbaId"] = new SelectList(_context.StudentskeSluzbe, "Id", "Id", dokument.StudentskaSluzbaId);
            return View(dokument);
        }

        // POST: Dokumenti/edit/5
        [HttpPost]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Naziv,Putanja,brojIndeksa,StudentskaSluzbaId")] Dokument dokument)
        {
            if (id != dokument.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dokument);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Dokumenti.Any(e => e.Id == dokument.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(dokument);
        }

        // GET: Dokumenti/delete/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null) return NotFound();

            var dokument = await _context.Dokumenti
                .Include(d => d.Student)
                .Include(d => d.StudentskaSluzba)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (dokument == null) return NotFound();

            return View(dokument);
        }

        // POST: Dokumenti/delete/5
        [HttpPost, ActionName("Delete")]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var dokument = await _context.Dokumenti.FindAsync(id);
            if (dokument != null) _context.Dokumenti.Remove(dokument);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
