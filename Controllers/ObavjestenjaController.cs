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

        public async Task<IActionResult> Index(int? studijskiProgramId)
        {
            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv");

            var query = _context.Obavjestenja
                .Include(o => o.ObavjestenjeStudijskiProgrami)
                    .ThenInclude(osp => osp.StudijskiProgram)
                .Include(o => o.Korisnik)
                .OrderByDescending(o => o.DatumObjave)
                .AsQueryable();

            // Filtriraj po studijskom programu ako je definisan
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
                    AutorIme = o.Korisnik != null ? $"{o.Korisnik.Ime} {o.Korisnik.Prezime}" : "Nepoznato",
                    KorisnikAspNetUserId = o.Korisnik.AspNetUserId
                })
                .ToListAsync();

            return View(obavjestenja);
        }

        [HttpGet("Details/{id:long}")]
        [Authorize(Roles = "Student, Studentska služba, Profesor, Asistent")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var obavjestenje = await _context.Obavjestenja
                .Include(o => o.ObavjestenjeStudijskiProgrami)
                    .ThenInclude(osp => osp.StudijskiProgram)
                .Include(o => o.Korisnik)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (obavjestenje == null) return NotFound();

            var viewModel = new ObavjestenjeViewModel
            {
                Id = obavjestenje.Id,
                Naslov = obavjestenje.Naslov,
                Sadrzaj = obavjestenje.Sadrzaj,
                DatumObjave = obavjestenje.DatumObjave,
                StudijskiProgramNazivi = obavjestenje.ObavjestenjeStudijskiProgrami
                    .Select(osp => osp.StudijskiProgram.Naziv)
                    .ToList(),
                AutorIme = obavjestenje.Korisnik != null ? $"{obavjestenje.Korisnik.Ime} {obavjestenje.Korisnik.Prezime}" : "Nepoznato",
                AutorAspNetUserId = obavjestenje.Korisnik?.AspNetUserId
            };

            return View(viewModel);
        }

        // GET: Obavjestenja/Create
        [HttpGet("Create")]
        [Authorize(Roles = "Studentska služba, Profesor, Asistent")]
        public IActionResult Create()
        {
            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv");
            return View();
        }

        // POST: Obavjestenja/Create
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

                var aspNetUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var korisnik = await _context.Korisnici.FirstOrDefaultAsync(k => k.AspNetUserId == aspNetUserId);

                if (korisnik == null)
                {
                    return Unauthorized("Nije moguće pronaći prijavljenog korisnika.");
                }

                var obavjestenje = new Obavjestenje
                {
                    Naslov = dto.Naslov,
                    Sadrzaj = dto.Sadrzaj,
                    DatumObjave = DateTime.Now,
                    KorisnikId = korisnik.Id
                };

                _context.Obavjestenja.Add(obavjestenje);
                await _context.SaveChangesAsync();

                var obavjestenjeStudijskiProgrami = validniProgrami
                    .Select(id => new ObavjestenjeStudijskiProgram
                    {
                        ObavjestenjeId = obavjestenje.Id,
                        StudijskiProgramId = id
                    })
                    .ToList();

                _context.ObavjestenjeStudijskiProgrami.AddRange(obavjestenjeStudijskiProgrami);
                await _context.SaveChangesAsync();

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

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (obavjestenje.Korisnik.AspNetUserId != currentUserId)
            {
                return Forbid(); // Korisnik može uređivati samo svoja obavještenja
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

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (obavjestenje.Korisnik.AspNetUserId != currentUserId)
            {
                return Forbid();
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
                .FirstOrDefaultAsync(m => m.Id == id);

            if (obavjestenje == null) return NotFound();

            if (!await DaLiJeKreatorObavjestenjaAsync(obavjestenje))
            {
                return Forbid();
            }

            return View(obavjestenje);
        }

        // POST: Obavjestenja/Delete/{id}
        [HttpPost("Delete/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba, Profesor, Asistent")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var obavjestenje = await _context.Obavjestenja.FindAsync(id);
            if (obavjestenje == null)
            {
                return NotFound();
            }

            if (!await DaLiJeKreatorObavjestenjaAsync(obavjestenje))
            {
                return Forbid();
            }

            _context.Obavjestenja.Remove(obavjestenje);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Pomoćna metoda za proveru vlasništva
        private async Task<bool> DaLiJeKreatorObavjestenjaAsync(Obavjestenje obavjestenje)
        {
            var currentAspNetUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var kreator = await _context.Korisnici.FirstOrDefaultAsync(k => k.Id == obavjestenje.KorisnikId);

            return kreator != null && kreator.AspNetUserId == currentAspNetUserId;
        }

        private bool ObavjestenjeExists(long id)
        {
            return _context.Obavjestenja.Any(e => e.Id == id);
        }
    }
}