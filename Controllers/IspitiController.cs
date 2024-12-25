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
    [Route("Ispiti")]
    public class IspitiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IspitiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Ispiti
        [HttpGet]
        [Route("")]
        [Route("[Controller]/[Action]")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Ispiti.Include(i => i.Asistent).Include(i => i.Predmet).Include(i => i.Profesor);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Ispiti/Details/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ispit = await _context.Ispiti
                .Include(i => i.Asistent)
                .Include(i => i.Predmet)
                .Include(i => i.Profesor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ispit == null)
            {
                return NotFound();
            }

            return View(ispit);
        }

        // GET: Ispiti/Create
        [HttpGet]
        [Route("[Controller]/[Action]")]
        public IActionResult Create()
        {
            ViewData["AsistentId"] = new SelectList(_context.Asistenti, "Id", "Id");
            ViewData["PredmetId"] = new SelectList(_context.Predmeti, "Id", "Id");
            ViewData["ProfesorId"] = new SelectList(_context.Profesori, "Id", "Id");
            return View();
        }

        // POST: Ispiti/Create
        [HttpPost]
        [Route("[Controller]/[Action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,datumOdrzavanja,Lokacija,brojBodova,PredmetId,ProfesorId,AsistentId")] Ispit ispit)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ispit);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AsistentId"] = new SelectList(_context.Asistenti, "Id", "Id", ispit.AsistentId);
            ViewData["PredmetId"] = new SelectList(_context.Predmeti, "Id", "Id", ispit.PredmetId);
            ViewData["ProfesorId"] = new SelectList(_context.Profesori, "Id", "Id", ispit.ProfesorId);
            return View(ispit);
        }

        // GET: Ispiti/Edit/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ispit = await _context.Ispiti.FindAsync(id);
            if (ispit == null)
            {
                return NotFound();
            }
            ViewData["AsistentId"] = new SelectList(_context.Asistenti, "Id", "Id", ispit.AsistentId);
            ViewData["PredmetId"] = new SelectList(_context.Predmeti, "Id", "Id", ispit.PredmetId);
            ViewData["ProfesorId"] = new SelectList(_context.Profesori, "Id", "Id", ispit.ProfesorId);
            return View(ispit);
        }

        // POST: Ispiti/Edit/5
        [HttpPost]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,datumOdrzavanja,Lokacija,brojBodova,PredmetId,ProfesorId,AsistentId")] Ispit ispit)
        {
            if (id != ispit.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ispit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IspitExists(ispit.Id))
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
            ViewData["AsistentId"] = new SelectList(_context.Asistenti, "Id", "Id", ispit.AsistentId);
            ViewData["PredmetId"] = new SelectList(_context.Predmeti, "Id", "Id", ispit.PredmetId);
            ViewData["ProfesorId"] = new SelectList(_context.Profesori, "Id", "Id", ispit.ProfesorId);
            return View(ispit);
        }

        // GET: Ispiti/Delete/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ispit = await _context.Ispiti
                .Include(i => i.Asistent)
                .Include(i => i.Predmet)
                .Include(i => i.Profesor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ispit == null)
            {
                return NotFound();
            }

            return View(ispit);
        }

        // POST: Ispiti/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var ispit = await _context.Ispiti.FindAsync(id);
            if (ispit != null)
            {
                _context.Ispiti.Remove(ispit);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        [Route("[Controller]/Pretraga")]
        public async Task<IActionResult> Pretraga(string? predmetNaziv, string? profesorPrezime, DateTime? datum)
        {
            var query = _context.Ispiti
                .Include(i => i.Asistent)
                .Include(i => i.Predmet)
                .Include(i => i.Profesor)
                .AsQueryable();

            if (!string.IsNullOrEmpty(predmetNaziv))
            {
                query = query.Where(i => i.Predmet.Naziv.Contains(predmetNaziv));
            }

            if (!string.IsNullOrEmpty(profesorPrezime))
            {
                query = query.Where(i => i.Profesor.Prezime.Contains(profesorPrezime));
            }

            if (datum.HasValue)
            {
                query = query.Where(i => i.datumOdrzavanja.Date == datum.Value.Date);
            }

            return View("Index", await query.ToListAsync());
        }


        private bool IspitExists(long id)
        {
            return _context.Ispiti.Any(e => e.Id == id);
        }
    }
}
