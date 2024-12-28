using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;

namespace StudentHub.Controllers
{
    [Route("Predmeti")]
    public class PredmetiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PredmetiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Predmeti
        [HttpGet]
        [Route("")]
        [Route("[Controller]/[Action]")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Predmeti
                .Include(p => p.Asistent)
                .Include(p => p.NastavniPlan)
                .Include(p => p.Profesor);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Predmeti/Details/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var predmet = await _context.Predmeti
                .Include(p => p.Asistent)
                .Include(p => p.NastavniPlan)
                .Include(p => p.Profesor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (predmet == null) return NotFound();

            return View(predmet);
        }

        // GET: Predmeti/Create
        [HttpGet]
        [Route("[Controller]/[Action]")]
        public IActionResult Create()
        {
            ViewData["ProfesorId"] = new SelectList(_context.Profesori, "Id", "Ime");
            ViewData["AsistentId"] = new SelectList(_context.Asistenti, "Id", "Ime");
            ViewData["NastavniPlanId"] = new SelectList(_context.NastavniPlanovi, "Id", "Naziv");
            return View();
        }

        // POST: Predmeti/Create
        [HttpPost]
        [Route("[Controller]/[Action]")]
        public async Task<IActionResult> Create([Bind("Naziv,Opis,ECTS,ProfesorId,AsistentId,NastavniPlanId")] Predmet predmet)
        {
            if (ModelState.IsValid)
            {
                _context.Add(predmet);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProfesorId"] = new SelectList(_context.Profesori, "Id", "Ime", predmet.ProfesorId);
            ViewData["AsistentId"] = new SelectList(_context.Asistenti, "Id", "Ime", predmet.AsistentId);
            ViewData["NastavniPlanId"] = new SelectList(_context.NastavniPlanovi, "Id", "Naziv", predmet.NastavniPlanId);
            return View(predmet);
        }

        // GET: Predmeti/Edit/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var predmet = await _context.Predmeti.FindAsync(id);
            if (predmet == null) return NotFound();

            ViewData["ProfesorId"] = new SelectList(_context.Profesori, "Id", "Ime", predmet.ProfesorId);
            ViewData["AsistentId"] = new SelectList(_context.Asistenti, "Id", "Ime", predmet.AsistentId);
            ViewData["NastavniPlanId"] = new SelectList(_context.NastavniPlanovi, "Id", "Naziv", predmet.NastavniPlanId);
            return View(predmet);
        }

        // POST: Predmeti/Edit/5
        [HttpPost]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Naziv,Opis,ECTS,ProfesorId,AsistentId,NastavniPlanId")] Predmet predmet)
        {
            if (id != predmet.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(predmet);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PredmetExists(predmet.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProfesorId"] = new SelectList(_context.Profesori, "Id", "Ime", predmet.ProfesorId);
            ViewData["AsistentId"] = new SelectList(_context.Asistenti, "Id", "Ime", predmet.AsistentId);
            ViewData["NastavniPlanId"] = new SelectList(_context.NastavniPlanovi, "Id", "Naziv", predmet.NastavniPlanId);
            return View(predmet);
        }

        // GET: Predmeti/Delete/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var predmet = await _context.Predmeti
                .Include(p => p.Asistent)
                .Include(p => p.NastavniPlan)
                .Include(p => p.Profesor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (predmet == null)
            {
                return NotFound();
            }

            return View(predmet);
        }

        // POST: Predmeti/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("[Controller]/[Action]/{id?}")]

        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var predmet = await _context.Predmeti.FindAsync(id);
            if (predmet != null)
            {
                _context.Predmeti.Remove(predmet);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PredmetExists(long id)
        {
            return _context.Predmeti.Any(e => e.Id == id);
        }
    }
}
