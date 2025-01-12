using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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
            var applicationDbContext = _context.Ispiti
                .Include(i => i.Asistent)
                .Include(i => i.Predmet)
                .Include(i => i.Profesor);
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
            if (ispit == null) return NotFound();

            return View(ispit);
        }

        // GET: Ispiti/Create
        [HttpGet]
        [Route("[Controller]/[Action]")]
        public IActionResult Create()
        {
            ViewData["PredmetId"] = new SelectList(_context.Predmeti, "Id", "Naziv");
            ViewData["ProfesorId"] = new SelectList(_context.Profesori, "Id", "Titula", "Ime", "Prezime");
            ViewData["AsistentId"] = new SelectList(_context.Asistenti, "Id", "Titula", "Ime", "Prezime");
            return View();
        }

        // POST: Ispiti/Create
        [HttpPost]
        [Route("[Controller]/[Action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id, Predmet, Lokacija, BrojBodova, DatumOdrzavanja, DatumObjave")] Ispit ispit)
        {
            if (ModelState.IsValid)
            {
                ispit.DatumObjave = DateTime.Now;
                SetUserRoleIds(ispit);

                _context.Add(ispit);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["PredmetId"] = new SelectList(_context.Predmeti, "Id", "Naziv", ispit.PredmetId);
            return View(ispit);
        }

        // GET: Ispiti/Edit/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var ispit = await _context.Ispiti.FindAsync(id);
            if (ispit == null) return NotFound();

            ViewData["PredmetId"] = new SelectList(_context.Predmeti, "Id", "Naziv", ispit.PredmetId);
            return View(ispit);
        }

        // POST: Ispiti/Edit/5
        [HttpPost]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,DatumOdrzavanja,Lokacija,BrojBodova,PredmetId")] Ispit ispit)
        {
            if (id != ispit.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    ispit.DatumObjave = DateTime.Now;
                    SetUserRoleIds(ispit);

                    _context.Update(ispit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IspitExists(ispit.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PredmetId"] = new SelectList(_context.Predmeti, "Id", "Naziv", ispit.PredmetId);
            return View(ispit);
        }

        // GET: Ispiti/Delete/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null) return NotFound();

            var ispit = await _context.Ispiti
                .Include(i => i.Asistent)
                .Include(i => i.Predmet)
                .Include(i => i.Profesor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ispit == null) return NotFound();
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
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool IspitExists(long id)
        {
            return _context.Ispiti.Any(e => e.Id == id);
        }

        // Pomoćna metoda za postavljanje korisničkih ID-ova i uloga
        private void SetUserRoleIds(Ispit ispit)
        {
            if (User.IsInRole("Studentska služba"))
            {
                ispit.StudentskaSluzbaId = GetCurrentUserId();
            }
            else if (User.IsInRole("Profesor"))
            {
                ispit.ProfesorId = GetCurrentUserId();
            }
            else if (User.IsInRole("Asistent"))
            {
                ispit.AsistentId = GetCurrentUserId();
            }
        }

        // Metoda za dohvaćanje trenutnog korisničkog ID-a
        private long GetCurrentUserId()
        {
            return long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}
