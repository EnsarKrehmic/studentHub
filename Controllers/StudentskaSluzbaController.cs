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
    [Route("StudentskaSluzba")]
    public class StudentskaSluzbaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentskaSluzbaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: StudentskaSluzba
        [HttpGet]
        [Route("")]
        [Route("[Controller]/[Action]")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.StudentskeSluzbe.Include(s => s.Predmet).Include(s => s.Zahtjev);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: StudentskaSluzba/Details/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studentskaSluzba = await _context.StudentskeSluzbe
                .Include(s => s.Predmet)
                .Include(s => s.Zahtjev)
                .FirstOrDefaultAsync(m => m.JMBG == id);
            if (studentskaSluzba == null)
            {
                return NotFound();
            }

            return View(studentskaSluzba);
        }

        // GET: StudentskaSluzba/Create
        [HttpGet]
        [Route("[Controller]/[Action]")]
        public IActionResult Create()
        {
            ViewData["JMBG"] = new SelectList(_context.Korisnici, "JMBG", "JMBG");
            ViewData["PredmetId"] = new SelectList(_context.Predmeti, "Id", "Id");
            ViewData["ZahtjevId"] = new SelectList(_context.Zahtjevi, "Id", "Id");
            return View();
        }

        // POST: StudentskaSluzba/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("[Controller]/[Action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Ime,Prezime,Email,Lozinka,JMBG,ZahtjevId,PredmetId")] StudentskaSluzba studentskaSluzba)
        {
            if (ModelState.IsValid)
            {
                _context.Add(studentskaSluzba);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["JMBG"] = new SelectList(_context.Korisnici, "JMBG", "JMBG", studentskaSluzba.JMBG);
            ViewData["PredmetId"] = new SelectList(_context.Predmeti, "Id", "Id", studentskaSluzba.PredmetId);
            ViewData["ZahtjevId"] = new SelectList(_context.Zahtjevi, "Id", "Id", studentskaSluzba.ZahtjevId);
            return View(studentskaSluzba);
        }

        // GET: StudentskaSluzba/Edit/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studentskaSluzba = await _context.StudentskeSluzbe.FindAsync(id);
            if (studentskaSluzba == null)
            {
                return NotFound();
            }
            ViewData["JMBG"] = new SelectList(_context.Korisnici, "JMBG", "JMBG", studentskaSluzba.JMBG);
            ViewData["PredmetId"] = new SelectList(_context.Predmeti, "Id", "Id", studentskaSluzba.PredmetId);
            ViewData["ZahtjevId"] = new SelectList(_context.Zahtjevi, "Id", "Id", studentskaSluzba.ZahtjevId);
            return View(studentskaSluzba);
        }

        // POST: StudentskaSluzba/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Ime,Prezime,Email,Lozinka,JMBG,ZahtjevId,PredmetId")] StudentskaSluzba studentskaSluzba)
        {
            if (id != studentskaSluzba.JMBG)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(studentskaSluzba);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentskaSluzbaExists(studentskaSluzba.JMBG))
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
            ViewData["JMBG"] = new SelectList(_context.Korisnici, "JMBG", "JMBG", studentskaSluzba.JMBG);
            ViewData["PredmetId"] = new SelectList(_context.Predmeti, "Id", "Id", studentskaSluzba.PredmetId);
            ViewData["ZahtjevId"] = new SelectList(_context.Zahtjevi, "Id", "Id", studentskaSluzba.ZahtjevId);
            return View(studentskaSluzba);
        }

        // GET: StudentskaSluzba/Delete/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studentskaSluzba = await _context.StudentskeSluzbe
                .Include(s => s.Predmet)
                .Include(s => s.Zahtjev)
                .FirstOrDefaultAsync(m => m.JMBG == id);
            if (studentskaSluzba == null)
            {
                return NotFound();
            }

            return View(studentskaSluzba);
        }

        // POST: StudentskaSluzba/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var studentskaSluzba = await _context.StudentskeSluzbe.FindAsync(id);
            if (studentskaSluzba != null)
            {
                _context.StudentskeSluzbe.Remove(studentskaSluzba);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudentskaSluzbaExists(long id)
        {
            return _context.StudentskeSluzbe.Any(e => e.JMBG == id);
        }
    }
}
