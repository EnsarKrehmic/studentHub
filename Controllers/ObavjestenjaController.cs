using System.Security.Claims;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;

namespace StudentHub.Controllers
{
    [Route("Obavjestenja")]
    public class ObavjestenjaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ObavjestenjaController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int? studijskiProgramId)
        {
            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv");

            var obavjestenja = _context.Obavjestenja
                .Include(o => o.Asistent)
                .Include(o => o.Profesor)
                .Include(o => o.StudentskaSluzba)
                .Include(o => o.StudijskiProgram)
                .AsQueryable();

            if (studijskiProgramId.HasValue)
            {
                obavjestenja = obavjestenja.Where(o => o.StudijskiProgramId == studijskiProgramId);
            }

            return View(await obavjestenja.ToListAsync());
        }

        // GET: Obavjestenja/Details/{id}
        [HttpGet("Details/{id:long}")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var obavjestenje = await _context.Obavjestenja
                .Include(o => o.Asistent)
                .Include(o => o.Profesor)
                .Include(o => o.StudentskaSluzba)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (obavjestenje == null) return NotFound();

            return View(obavjestenje);
        }

        // GET: Obavjestenja/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv");
            return View();
        }

        // POST: Obavjestenja/Create
        [HttpPost("Create")]
        public async Task<IActionResult> Create(ObavjestenjeCreateViewModel dto)
        {
            // Provera validnosti modela
            if (ModelState.IsValid)
            {
                Console.WriteLine("Uneti podaci za obavještenje:");

                // Ispis atributa
                Console.WriteLine($"Naslov: {dto.Naslov}");
                Console.WriteLine($"Sadrzaj: {dto.Sadrzaj}");
                Console.WriteLine($"StudijskiProgramId: {dto.StudijskiProgramId}");

                dto.DatumObjave = DateTime.Now;

                // Logika za autentifikovanog korisnika
                if (User.Identity?.IsAuthenticated == true)
                {
                    Console.WriteLine($"Autentifikovan korisnik: {User.Identity.Name}");
                }
                else
                {
                    Console.WriteLine("Korisnik nije autentifikovan.");
                }

                var obavjestenje = new Obavjestenje
                {
                    Naslov = dto.Naslov,
                    Sadrzaj = dto.Sadrzaj,
                    StudijskiProgramId = dto.StudijskiProgramId,
                    DatumObjave = DateTime.Now
                };

                _context.Add(obavjestenje);
                await _context.SaveChangesAsync();
                Console.WriteLine("Obavještenje je uspešno sačuvano.");
                return RedirectToAction(nameof(Index));
            }

            // Ispis grešaka validacije
            Console.WriteLine("ModelState nije validan. Greške:");
            foreach (var error in ModelState)
            {
                Console.WriteLine($"{error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
            }

            // Ponovno postavljanje ViewBag-a za dropdown listu
            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", dto.StudijskiProgramId);

            return View(dto);
        }

        // GET: Obavjestenja/Edit/5
        [HttpGet("Edit/{id:long}")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var obavjestenje = await _context.Obavjestenja.FindAsync(id);
            if (obavjestenje == null)
            {
                return NotFound();
            }
            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", obavjestenje.StudijskiProgramId);
            return View(obavjestenje);
        }

        // POST: Obavjestenja/Edit/{id}
        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Naslov,Sadrzaj,StudijskiProgram")] Obavjestenje obavjestenje)
        {
            if (id != obavjestenje.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(obavjestenje);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ObavjestenjeExists(obavjestenje.Id))
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
            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", obavjestenje.StudijskiProgramId);
            return View(obavjestenje);
        }

        // GET: Obavjestenja/Delete/{id}
        [HttpGet("Delete/{id:long}")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null) return NotFound();

            var obavjestenje = await _context.Obavjestenja
                .Include(o => o.Asistent)
                .Include(o => o.Profesor)
                .Include(o => o.StudentskaSluzba)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (obavjestenje == null) return NotFound();
            return View(obavjestenje);
        }

        // POST: Obavjestenja/Delete/{id}
        [HttpPost("Delete/{id:long}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var obavjestenje = await _context.Obavjestenja.FindAsync(id);
            if (obavjestenje != null)
            {
                _context.Obavjestenja.Remove(obavjestenje);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ObavjestenjeExists(long id)
        {
            return _context.Obavjestenja.Any(e => e.Id == id);
        }

        // Pomoćna metoda za postavljanje korisničkih ID-ova i uloga
        private void SetUserRoleIds(Obavjestenje obavjestenje)
        {
            Console.WriteLine("Postavljanje korisnika za obavještenje...");
            if (User.IsInRole("StudentskaSluzba"))
            {
                obavjestenje.StudentskaSluzbaId = GetCurrentUserId();
            }
            else if (User.IsInRole("Profesor"))
            {
                obavjestenje.ProfesorId = GetCurrentUserId();
            }
            else if (User.IsInRole("Asistent"))
            {
                obavjestenje.AsistentId = GetCurrentUserId();
            }
        }

        // Metoda za dohvaćanje trenutnog korisničkog ID-a
        private long GetCurrentUserId()
        {
            return long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}