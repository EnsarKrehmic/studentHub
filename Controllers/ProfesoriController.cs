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
    [Route("Profesori")]
    public class ProfesoriController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfesoriController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Profesori
        [HttpGet]
        [Route("")]
        [Route("[Controller]/[Action]")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Profesori;
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Profesori/Details/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profesor = await _context.Profesori
                .FirstOrDefaultAsync(m => m.JMBG == id);
            if (profesor == null)
            {
                return NotFound();
            }

            return View(profesor);
        }

        // GET: Profesori/Create
        [HttpGet]
        [Route("[Controller]/[Action]")]
        public IActionResult Create()
        {
            ViewData["JMBG"] = new SelectList(_context.Korisnici, "JMBG", "JMBG");
            return View();
        }

        // POST: Profesori/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("[Controller]/[Action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Email,Titula,JMBG")] Profesor profesor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(profesor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["JMBG"] = new SelectList(_context.Korisnici, "JMBG", "JMBG", profesor.JMBG);
            return View(profesor);
        }

        // GET: Profesori/Edit/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profesor = await _context.Profesori.FindAsync(id);
            if (profesor == null)
            {
                return NotFound();
            }
            ViewData["JMBG"] = new SelectList(_context.Korisnici, "JMBG", "JMBG", profesor.JMBG);
            return View(profesor);
        }

        // POST: Profesori/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Email,Titula,JMBG")] Profesor profesor)
        {
            if (id != profesor.JMBG)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(profesor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProfesorExists(profesor.JMBG))
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
            ViewData["JMBG"] = new SelectList(_context.Korisnici, "JMBG", "JMBG", profesor.JMBG);
            return View(profesor);
        }

        // GET: Profesori/Delete/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profesor = await _context.Profesori
                .FirstOrDefaultAsync(m => m.JMBG == id);
            if (profesor == null)
            {
                return NotFound();
            }

            return View(profesor);
        }

        // POST: Profesori/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var profesor = await _context.Profesori.FindAsync(id);
            if (profesor != null)
            {
                _context.Profesori.Remove(profesor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProfesorExists(long id)
        {
            return _context.Profesori.Any(e => e.JMBG == id);
        }
    }
}
