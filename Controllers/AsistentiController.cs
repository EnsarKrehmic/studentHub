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
    [Route("Asistenti")]
    public class AsistentiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AsistentiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Asistenti
        [HttpGet]
        [Route("")]
        [Route("[Controller]/[Action]")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Asistenti;
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Asistenti/Details/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asistent = await _context.Asistenti
                .FirstOrDefaultAsync(m => m.JMBG == id);
            if (asistent == null)
            {
                return NotFound();
            }

            return View(asistent);
        }

        // GET: Asistenti/Create
        [HttpGet]
        [Route("[Controller]/[Action]")]
        public IActionResult Create()
        {
            ViewData["JMBG"] = new SelectList(_context.Korisnici, "JMBG", "JMBG");
            return View();
        }

        // POST: Asistenti/Create
        [HttpPost]
        [Route("[Controller]/[Action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Email,Titula,JMBG")] Asistent asistent)
        {
            if (ModelState.IsValid)
            {
                _context.Add(asistent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["JMBG"] = new SelectList(_context.Korisnici, "JMBG", "JMBG", asistent.JMBG);
            return View(asistent);
        }

        // GET: Asistenti/Edit/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asistent = await _context.Asistenti.FindAsync(id);
            if (asistent == null)
            {
                return NotFound();
            }
            ViewData["JMBG"] = new SelectList(_context.Korisnici, "JMBG", "JMBG", asistent.JMBG);
            return View(asistent);
        }

        // POST: Asistenti/Edit/5
        [HttpPost]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Email,Titula,JMBG")] Asistent asistent)
        {
            if (id != asistent.JMBG)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(asistent);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AsistentExists(asistent.JMBG))
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
            ViewData["JMBG"] = new SelectList(_context.Korisnici, "JMBG", "JMBG", asistent.JMBG);
            return View(asistent);
        }

        // GET: Asistenti/Delete/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asistent = await _context.Asistenti
                .FirstOrDefaultAsync(m => m.JMBG == id);
            if (asistent == null)
            {
                return NotFound();
            }

            return View(asistent);
        }

        // POST: Asistenti/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var asistent = await _context.Asistenti.FindAsync(id);
            if (asistent != null)
            {
                _context.Asistenti.Remove(asistent);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AsistentExists(long id)
        {
            return _context.Asistenti.Any(e => e.JMBG == id);
        }
    }
}
