using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;

namespace StudentHub.Controllers
{
    [Route("Zahtjevi")]
    public class ZahtjeviController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ZahtjeviController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Zahtjevi
        [HttpGet]
        [Route("")]
        [Route("[Controller]/[Action]")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Zahtjevi
                .Include(z => z.Student);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Zahtjevi/Details/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

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
        [HttpGet]
        [Route("[Controller]/[Action]")]
        public IActionResult Create()
        {
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "BrojIndeksa");
            return View();
        }

        // POST: Zahtjevi/Create
        [HttpPost]
        [Route("[Controller]/[Action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TipZahtjeva,StatusZahtjeva,DatumPodnosenja,DatumRjesavanja,BrojIndeksa,StudentId")] Zahtjev zahtjev)
        {
            if (ModelState.IsValid)
            {
                _context.Add(zahtjev);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "BrojIndeksa", zahtjev.StudentId);
            return View(zahtjev);
        }

        // GET: Zahtjevi/Edit/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var zahtjev = await _context.Zahtjevi.FindAsync(id);
            if (zahtjev == null)
            {
                return NotFound();
            }
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "BrojIndeksa", zahtjev.StudentId);
            return View(zahtjev);
        }

        // POST: Zahtjevi/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("TipZahtjeva, StatusZahtjeva, DatumPodnosenja, DatumRjesavanja, BrojIndeksa, StudentId")] Zahtjev zahtjev)
        {
            if (id != zahtjev.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(zahtjev);
                    await _context.SaveChangesAsync();
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "BrojIndeksa", zahtjev.StudentId);
            return View(zahtjev);
        }

        // GET: Zahtjevi/Delete/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var zahtjev = await _context.Zahtjevi
                .Include(z => z.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (zahtjev == null)
            {
                return NotFound();
            }

            return View(zahtjev);
        }

        // POST: Zahtjevi/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var zahtjev = await _context.Zahtjevi.FindAsync(id);
            if (zahtjev != null)
            {
                _context.Zahtjevi.Remove(zahtjev);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ZahtjevExists(long id)
        {
            return _context.Zahtjevi.Any(e => e.Id == id);
        }
    }
}
