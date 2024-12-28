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
                .FirstOrDefaultAsync(m => m.Id == id);
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
            ViewData["AsistentId"] = new SelectList(_context.Korisnici, "Id", "Id");
            return View();
        }

        // POST: Asistenti/Create
        [HttpPost]
        [Route("[Controller]/[Action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,JMBG,Ime,Prezime,Email,Lozinka,Titula")] Asistent asistent)
        {
            // Proveri da li već postoji korisnik sa datim JMBG
            var postojiKorisnik = await _context.Korisnici
                .AnyAsync(k => k.JMBG == asistent.JMBG);

            if (postojiKorisnik)
            {
                ModelState.AddModelError("JMBG", "Korisnik sa ovim JMBG-om već postoji.");
                return View(asistent);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Asistenti.Add(asistent);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Došlo je do greške: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "Došlo je do greške prilikom kreiranja asistenta.");
                }
            }
            ViewData["AsistentId"] = new SelectList(_context.Korisnici, "Id", "Id", asistent.Id);            
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
            ViewData["AsistentId"] = new SelectList(_context.Korisnici, "Id", "Id", asistent.Id);
            return View(asistent);
        }

        // POST: Asistenti/Edit/5
        [HttpPost]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,JMBG,Ime,Prezime,Email,Lozinka,Titula")] Asistent asistent)
        {
            if (id != asistent.Id)
            {
                return NotFound();
            }

            // Provjera da li postoji drugi korisnik sa istim JMBG osim trenutnog
            if (_context.Korisnici.Any(k => k.JMBG == asistent.JMBG && k.Id != id))
            {
                ModelState.AddModelError("JMBG", "Korisnik sa ovim JMBG-om već postoji.");
                return View(asistent);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(asistent);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AsistentExists(asistent.Id))
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
                    ModelState.AddModelError(string.Empty, "Došlo je do greške prilikom ažuriranja asistenta.");
                }
            }
            ViewData["AsistentId"] = new SelectList(_context.Korisnici, "Id", "Id", asistent.Id);
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
                .FirstOrDefaultAsync(m => m.Id == id);
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
                var korisnik = await _context.Korisnici.FindAsync(id);
                if (korisnik != null)
                {
                    _context.Korisnici.Remove(korisnik);
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AsistentExists(long id)
        {
            return _context.Asistenti.Any(e => e.Id == id);
        }
    }
}