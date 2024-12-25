using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            var applicationDbContext = _context.Zahtjevi.Include(z => z.Student);
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
            ViewData["brojIndeksa"] = new SelectList(_context.Studenti, "brojIndeksa", "brojIndeksa");
            return View();
        }

        // POST: Zahtjevi/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("[Controller]/[Action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,tipZahtjeva,statusZahtjeva,datumPodnosenja,datumRjesavanja,brojIndeksa")] Zahtjev zahtjev)
        {
            if (ModelState.IsValid)
            {
                _context.Add(zahtjev);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["brojIndeksa"] = new SelectList(_context.Studenti, "brojIndeksa", "brojIndeksa", zahtjev.brojIndeksa);
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
            ViewData["brojIndeksa"] = new SelectList(_context.Studenti, "brojIndeksa", "brojIndeksa", zahtjev.brojIndeksa);
            return View(zahtjev);
        }

        // POST: Zahtjevi/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,tipZahtjeva,statusZahtjeva,datumPodnosenja,datumRjesavanja,brojIndeksa")] Zahtjev zahtjev)
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
            ViewData["brojIndeksa"] = new SelectList(_context.Studenti, "brojIndeksa", "brojIndeksa", zahtjev.brojIndeksa);
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
