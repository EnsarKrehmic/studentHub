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
                .FirstOrDefaultAsync(m => m.Id == id);
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
            ViewData["ProfesorId"] = new SelectList(_context.Korisnici, "Id", "Ime");
            return View();
        }

        // POST: Profesori/Create
        [HttpPost]
        [Route("[Controller]/[Action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("JMBG,Ime,Prezime,Email,Lozinka,Titula")] Profesor profesor)
        {
            // Proveri da li već postoji korisnik sa datim JMBG
            var postojiKorisnik = await _context.Korisnici
                .AnyAsync(k => k.JMBG == profesor.JMBG);

            if (postojiKorisnik)
            {
                ModelState.AddModelError("JMBG", "Korisnik sa ovim JMBG-om već postoji.");
                return View(profesor);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Profesori.Add(profesor);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Došlo je do greške: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "Došlo je do greške prilikom kreiranja profesora.");
                }
            }
            ViewData["ProfesorId"] = new SelectList(_context.Korisnici, "Id", "Ime", profesor.Id);
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
            ViewData["ProfesorId"] = new SelectList(_context.Korisnici, "Id", "Ime", profesor.Id);
            return View(profesor);
        }

        // POST: Profesori/Edit/5
        [HttpPost]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("JMBG,Ime,Prezime,Email,Lozinka,Titula")] Profesor profesor)
        {
            if (id != profesor.Id)
            {
                return NotFound();
            }

            // Provjera da li postoji drugi korisnik sa istim JMBG osim trenutnog
            if (_context.Korisnici.Any(k => k.JMBG == profesor.JMBG && k.Id != id))
            {
                ModelState.AddModelError("JMBG", "Korisnik sa ovim JMBG-om već postoji.");
                return View(profesor);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(profesor);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProfesorExists(profesor.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Došlo je do greške: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "Došlo je do greške prilikom ažuriranja profesora.");
                }
            }
            ViewData["ProfesorId"] = new SelectList(_context.Korisnici, "Id", "Ime", profesor.Id);
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
                .FirstOrDefaultAsync(m => m.Id == id);
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
            return _context.Profesori.Any(e => e.Id == id);
        }
    }
}