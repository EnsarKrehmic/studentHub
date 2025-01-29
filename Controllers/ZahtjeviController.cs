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
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var zahtjevi = await _context.Zahtjevi.Include(z => z.Student).ToListAsync();
            return View(zahtjevi);
        }

        // GET: Zahtjevi/Details/{id}
        [HttpGet("Details/{id:long}")]
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
        public IActionResult Create()
        {
            ViewBag.Studenti = new SelectList(_context.Studenti, "Id", "ImePrezime");
            return View();
        }

        // POST: Zahtjevi/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TipZahtjeva,StatusZahtjeva,DatumPodnosenja,DatumRjesavanja,StudentId")] Zahtjev zahtjev)
        {
            if (ModelState.IsValid)
            {
                _context.Zahtjevi.Add(zahtjev);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Studenti = new SelectList(_context.Studenti, "Id", "ImePrezime", zahtjev.StudentId);
            return View(zahtjev);
        }

        // GET: Zahtjevi/Edit/{id}
        [HttpGet("Edit/{id:long}")]
        public async Task<IActionResult> Edit(long id)
        {
            var zahtjev = await _context.Zahtjevi.FindAsync(id);
            if (zahtjev == null)
            {
                return NotFound();
            }
            ViewBag.Studenti = new SelectList(_context.Studenti, "Id", "ImePrezime", zahtjev.StudentId);
            return View(zahtjev);
        }

        // POST: Zahtjevi/Edit/{id}
        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,TipZahtjeva,StatusZahtjeva,DatumPodnosenja,DatumRjesavanja,StudentId")] Zahtjev zahtjev)
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
            ViewBag.Studenti = new SelectList(_context.Studenti, "Id", "ImePrezime", zahtjev.StudentId);
            return View(zahtjev);
        }

        // GET: Zahtjevi/Delete/{id}
        [HttpGet("Delete/{id:long}")]
        public async Task<IActionResult> Delete(long id)
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

        // POST: Zahtjevi/Delete/{id}
        [HttpPost("Delete/{id:long}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var zahtjev = await _context.Zahtjevi.FindAsync(id);
            if (zahtjev != null)
            {
                _context.Zahtjevi.Remove(zahtjev);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ZahtjevExists(long id)
        {
            return _context.Zahtjevi.Any(e => e.Id == id);
        }
    }
}
