using System.Security.Claims;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
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

            var query = _context.Obavjestenja
                .Include(o => o.ObavjestenjeStudijskiProgrami)
                    .ThenInclude(osp => osp.StudijskiProgram)
                .Include(o => o.Korisnik)
                .Include(o => o.StudentskaSluzba)
                .Include(o => o.Profesor)
                .Include(o => o.Asistent)
                .OrderByDescending(o => o.DatumObjave)
                .AsQueryable();

            // Ako je proslijeđen `studijskiProgramId`, filtriraj obavještenja po tom studijskom programu
            if (studijskiProgramId.HasValue)
            {
                query = query.Where(o => o.ObavjestenjeStudijskiProgrami
                    .Any(osp => osp.StudijskiProgramId == studijskiProgramId.Value));
            }

            var obavjestenja = await query
                .Select(o => new ObavjestenjeViewModel
                {
                    Id = o.Id,
                    Naslov = o.Naslov,
                    Sadrzaj = o.Sadrzaj,
                    DatumObjave = o.DatumObjave,
                    StudijskiProgramNazivi = o.ObavjestenjeStudijskiProgrami
                        .Select(osp => osp.StudijskiProgram.Naziv)
                        .ToList(),
                    AutorIme = o.Korisnik != null ? $"{o.Korisnik.Ime} {o.Korisnik.Prezime}" :
                                o.StudentskaSluzba != null ? $"{o.StudentskaSluzba.Ime} {o.StudentskaSluzba.Prezime}" :
                                o.Profesor != null ? $"{o.Profesor.Ime} {o.Profesor.Prezime}" :
                                o.Asistent != null ? $"{o.Asistent.Ime} {o.Asistent.Prezime}" : "Nepoznato"
                })
                .ToListAsync();

            return View(obavjestenja);
        }

        // GET: Obavjestenja/Details/{id}
        [HttpGet("Details/{id:long}")]
        [Authorize(Roles = "Student, Studentska služba, Profesor, Asistent")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var obavjestenje = await _context.Obavjestenja
                .Include(o => o.ObavjestenjeStudijskiProgrami)
                    .ThenInclude(osp => osp.StudijskiProgram)
                .Include(o => o.Asistent)
                .Include(o => o.Profesor)
                .Include(o => o.StudentskaSluzba)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (obavjestenje == null) return NotFound();

            return View(obavjestenje);
        }

        // GET: Obavjestenja/Create
        [HttpGet("Create")]
        [Authorize(Roles = "Studentska služba, Profesor, Asistent")]
        public IActionResult Create()
        {
            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv");
            return View();
        }

        [HttpPost("Create")]
        [Authorize(Roles = "Studentska služba, Profesor, Asistent")]
        public async Task<IActionResult> Create(ObavjestenjeCreateViewModel dto)
        {
            if (ModelState.IsValid)
            {
                var validniProgrami = await _context.StudijskiProgrami
                    .Where(sp => dto.StudijskiProgramiIds.Contains(sp.Id))
                    .Select(sp => sp.Id)
                    .ToListAsync();

                if (validniProgrami.Count != dto.StudijskiProgramiIds.Count)
                {
                    return BadRequest("Jedan ili više ID-jeva studijskih programa nisu validni.");
                }

                var korisnickoIme = User.Identity?.Name;
                var korisnik = await _context.Korisnici.FirstOrDefaultAsync(k => k.Email == korisnickoIme);
                var studentskaSluzba = await _context.StudentskeSluzbe.FirstOrDefaultAsync(s => s.Email == korisnickoIme);
                var profesor = await _context.Profesori.FirstOrDefaultAsync(p => p.Email == korisnickoIme);
                var asistent = await _context.Asistenti.FirstOrDefaultAsync(a => a.Email == korisnickoIme);

                var obavjestenje = new Obavjestenje
                {
                    Naslov = dto.Naslov,
                    Sadrzaj = dto.Sadrzaj,
                    DatumObjave = DateTime.Now,
                    KorisnikId = korisnik?.Id,
                    StudentskaSluzbaId = studentskaSluzba?.Id,
                    ProfesorId = profesor?.Id,
                    AsistentId = asistent?.Id
                };

                _context.Obavjestenja.Add(obavjestenje);
                await _context.SaveChangesAsync(); // Prvo sačuvaj obavještenje da dobije ID

                var obavjestenjeStudijskiProgrami = validniProgrami
                    .Select(id => new ObavjestenjeStudijskiProgram
                    {
                        ObavjestenjeId = obavjestenje.Id,
                        StudijskiProgramId = id
                    })
                    .ToList();

                _context.ObavjestenjeStudijskiProgrami.AddRange(obavjestenjeStudijskiProgrami);
                await _context.SaveChangesAsync(); // Sačuvaj povezane studijske programe

                return RedirectToAction(nameof(Index));
            }

            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv");
            return View(dto);
        }

        [HttpGet("Edit/{id:long}")]
        [Authorize(Roles = "Studentska služba, Profesor, Asistent")]
        public async Task<IActionResult> Edit(long id)
        {
            var obavjestenje = await _context.Obavjestenja
                .Include(o => o.ObavjestenjeStudijskiProgrami)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (obavjestenje == null)
            {
                return NotFound();
            }

            var viewModel = new ObavjestenjeCreateViewModel
            {
                Naslov = obavjestenje.Naslov,
                Sadrzaj = obavjestenje.Sadrzaj,
                DatumObjave = obavjestenje.DatumObjave,
                StudijskiProgramiIds = obavjestenje.ObavjestenjeStudijskiProgrami
                    .Select(sp => sp.StudijskiProgramId)
                    .ToList()
            };

            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv");
            return View(viewModel);
        }

        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba, Profesor, Asistent")]
        public async Task<IActionResult> Edit(long id, ObavjestenjeCreateViewModel dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv");
                return View(dto);
            }

            var obavjestenje = await _context.Obavjestenja
                .Include(o => o.ObavjestenjeStudijskiProgrami)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (obavjestenje == null)
            {
                return NotFound();
            }

            // Ažuriranje osnovnih podataka
            obavjestenje.Naslov = dto.Naslov;
            obavjestenje.Sadrzaj = dto.Sadrzaj;
            obavjestenje.DatumObjave = dto.DatumObjave;

            // Ažuriranje povezanih studijskih programa
            _context.ObavjestenjeStudijskiProgrami.RemoveRange(obavjestenje.ObavjestenjeStudijskiProgrami);

            var noviProgrami = dto.StudijskiProgramiIds.Select(id => new ObavjestenjeStudijskiProgram
            {
                ObavjestenjeId = obavjestenje.Id,
                StudijskiProgramId = id
            }).ToList();

            _context.ObavjestenjeStudijskiProgrami.AddRange(noviProgrami);

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Obavjestenja.Any(o => o.Id == obavjestenje.Id))
                {
                    return NotFound();
                }
                throw;
            }
        }

        // GET: Obavjestenja/Delete/{id}
        [HttpGet("Delete/{id:long}")]
        [Authorize(Roles = "Studentska služba, Profesor, Asistent")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null) return NotFound();

            var obavjestenje = await _context.Obavjestenja
                .Include(o => o.ObavjestenjeStudijskiProgrami)
                    .ThenInclude(osp => osp.StudijskiProgram)
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
        [Authorize(Roles = "Studentska služba, Profesor, Asistent")]
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