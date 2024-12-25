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
    [Route("Studenti")]
    public class StudentiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Studenti
        [HttpGet]
        [Route("")]
        [Route("[Controller]/[Action]")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Studenti;
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Studenti/Details/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Studenti
                .FirstOrDefaultAsync(m => m.JMBG == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Studenti/Create
        [HttpGet]
        [Route("[Controller]/[Action]")]
        public IActionResult Create()
        {
            ViewData["JMBG"] = new SelectList(_context.Korisnici, "JMBG", "JMBG");
            return View();
        }

        // POST: Studenti/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("[Controller]/[Action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("brojIndeksa,Email,predhodnoObrazovanje,studijskiProgram,godinaStudija,podaciUplata,JMBG")] Student student)
        {
            if (ModelState.IsValid)
            {
                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["JMBG"] = new SelectList(_context.Korisnici, "JMBG", "JMBG", student.JMBG);
            return View(student);
        }

        // GET: Studenti/Edit/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Studenti.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            ViewData["JMBG"] = new SelectList(_context.Korisnici, "JMBG", "JMBG", student.JMBG);
            return View(student);
        }

        // POST: Studenti/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("brojIndeksa,Email,predhodnoObrazovanje,studijskiProgram,godinaStudija,podaciUplata,JMBG")] Student student)
        {
            if (id != student.JMBG)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(student);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.JMBG))
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
            ViewData["JMBG"] = new SelectList(_context.Korisnici, "JMBG", "JMBG", student.JMBG);
            return View(student);
        }

        // GET: Studenti/Delete/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Studenti
                .FirstOrDefaultAsync(m => m.JMBG == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Studenti/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var student = await _context.Studenti.FindAsync(id);
            if (student != null)
            {
                _context.Studenti.Remove(student);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(long id)
        {
            return _context.Studenti.Any(e => e.JMBG == id);
        }
    }
}
