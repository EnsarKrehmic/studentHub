using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;

namespace StudentHub.Controllers
{
    [Route("Ocjene")]
    public class OcjeneController : Controller
    {
        private readonly ApplicationDbContext _context;
        public OcjeneController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Ocjene
        [HttpGet]
        [Route("")]
        [Route("[Controller]/[Action]")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Ocjene
                .Include(o => o.Predmet)
                .Include(o => o.Profesor)
                .Include(o => o.Student)
                .Select(o => new
                 {
                     o.Id,
                     o.Vrijednost,
                     PredmetNaziv = o.Predmet.Naziv ?? "N/A", // Handle null values
                     ProfesorIme = o.Profesor.Ime ?? "N/A", // Handle null values
                     StudentIme = o.Student.Ime ?? "N/A" // Handle null values
                 });
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Ocjene/Details/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ocjena = await _context.Ocjene
                .Include(o => o.Predmet)
                .Include(o => o.Profesor)
                .Include(o => o.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ocjena == null)
            {
                return NotFound();
            }

            return View(ocjena);
        }

        // GET: Ocjene/Create
        [HttpGet]
        [Route("[Controller]/[Action]")]
        public IActionResult Create()
        {
            ViewData["PredmetId"] = new SelectList(_context.Predmeti, "Id", "Naziv");
            ViewData["ProfesorId"] = new SelectList(_context.Profesori, "Id", "Ime");
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "brojIndeksa");
            return View();
        }

        // POST: Ocjene/Create
        [HttpPost]
        [Route("[Controller]/[Action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Vrijednost,PredmetId,brojIndeksa,StudentId,ProfesorId")] Ocjena ocjena)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ocjena);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PredmetId"] = new SelectList(_context.Predmeti, "Id", "Naziv", ocjena.PredmetId);
            ViewData["ProfesorId"] = new SelectList(_context.Profesori, "Id", "Ime", ocjena.ProfesorId);
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "brojIndeksa", ocjena.StudentId);
            return View(ocjena);
        }

        // GET: Ocjene/Edit/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ocjena = await _context.Ocjene.FindAsync(id);
            if (ocjena == null)
            {
                return NotFound();
            }
            ViewData["PredmetId"] = new SelectList(_context.Predmeti, "Id", "Naziv", ocjena.PredmetId);
            ViewData["ProfesorId"] = new SelectList(_context.Profesori, "Id", "Ime", ocjena.ProfesorId);
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "brojIndeksa", ocjena.StudentId);
            return View(ocjena);
        }

        // POST: Ocjene/Edit/5
        [HttpPost]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Vrijednost,PredmetId,brojIndeksa,StudentId,ProfesorId")] Ocjena ocjena)
        {
            if (id != ocjena.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ocjena);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OcjenaExists(ocjena.Id))
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
            ViewData["PredmetId"] = new SelectList(_context.Predmeti, "Id", "Naziv", ocjena.PredmetId);
            ViewData["ProfesorId"] = new SelectList(_context.Profesori, "Id", "Ime", ocjena.ProfesorId);
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "brojIndeksa", ocjena.StudentId);
            return View(ocjena);
        }

        // GET: Ocjene/Delete/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ocjena = await _context.Ocjene
                .Include(o => o.Predmet)
                .Include(o => o.Profesor)
                .Include(o => o.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ocjena == null)
            {
                return NotFound();
            }

            return View(ocjena);
        }

        // POST: Ocjene/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var ocjena = await _context.Ocjene.FindAsync(id);
            if (ocjena != null)
            {
                _context.Ocjene.Remove(ocjena);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OcjenaExists(long id)
        {
            return _context.Ocjene.Any(e => e.Id == id);
        }
    }
}