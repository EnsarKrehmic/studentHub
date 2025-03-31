using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;

namespace StudentHub.Controllers
{
    [Route("StudijskiProgrami")]
    public class StudijskiProgramiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudijskiProgramiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: StudijskiProgrami
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _context.StudijskiProgrami.ToListAsync());
        }

        [HttpGet("Details/{id:long}")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studijskiProgram = await _context.StudijskiProgrami
                .FirstOrDefaultAsync(m => m.Id == id);
            if (studijskiProgram == null)
            {
                return NotFound();
            }

            // Preuzimanje obavještenja povezanih sa studijskim programom
            var obavjestenja = await _context.Obavjestenja
                .Include(o => o.Korisnik)
                .Where(o => o.ObavjestenjeStudijskiProgrami.Any(osp => osp.StudijskiProgramId == id))
                .Select(o => new ObavjestenjeDetailsViewModel
                {
                    Id = o.Id,
                    Naslov = o.Naslov,
                    Sadrzaj = o.Sadrzaj,
                    DatumObjave = o.DatumObjave,
                    AutorIme = o.Korisnik != null ? $"{o.Korisnik.Ime} {o.Korisnik.Prezime}" : "Nepoznato"
                })
                .ToListAsync();

            // Brojanje korisnika povezanih sa studijskim programom
            int brojStudenata = await _context.StudentStudijskiProgrami
                .CountAsync(s => s.StudijskiProgramId == id);

            int brojProfesora = await _context.PredmetProfesori
                .Where(pp => pp.Predmet.NastavniPlan.StudijskiProgramId == id)
                .Select(pp => pp.ProfesorId)
                .Distinct()
                .CountAsync();

            int brojAsistenata = await _context.PredmetAsistenti
                .Where(pa => pa.Predmet.NastavniPlan.StudijskiProgramId == id)
                .Select(pa => pa.AsistentId)
                .Distinct()
                .CountAsync();

            // Kreiranje ViewModel-a
            var viewModel = new StudijskiProgramDetailsViewModel
            {
                StudijskiProgram = studijskiProgram,
                Obavjestenja = obavjestenja,
                BrojStudenata = brojStudenata,
                BrojProfesora = brojProfesora,
                BrojAsistenata = brojAsistenata
            };

            return View(viewModel);
        }


        // GET: StudijskiProgrami/Create{id}
        [HttpGet("Create")]
        [Authorize(Roles = "Studentska služba")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: StudijskiProgrami/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Create([Bind("Id,Naziv,Opis,TrajanjeUGodinama")] StudijskiProgram studijskiProgram)
        {
            if (ModelState.IsValid)
            {
                _context.Add(studijskiProgram);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(studijskiProgram);
        }

        // GET: StudijskiProgrami/Edit/{id}
        [HttpGet("Edit/{id:long}")]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studijskiProgram = await _context.StudijskiProgrami.FindAsync(id);
            if (studijskiProgram == null)
            {
                return NotFound();
            }
            return View(studijskiProgram);
        }

        // POST: StudijskiProgrami/Edit/5
        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Naziv,Opis,TrajanjeUGodinama")] StudijskiProgram studijskiProgram)
        {
            if (id != studijskiProgram.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(studijskiProgram);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudijskiProgramExists(studijskiProgram.Id))
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
            return View(studijskiProgram);
        }

        // GET: StudijskiProgrami/Delete/{id}
        [HttpGet("Delete/{id:long}")]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studijskiProgram = await _context.StudijskiProgrami
                .FirstOrDefaultAsync(m => m.Id == id);
            if (studijskiProgram == null)
            {
                return NotFound();
            }

            return View(studijskiProgram);
        }

        // POST: StudijskiProgrami/Delete/{id}
        [HttpPost("Delete/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var studijskiProgram = await _context.StudijskiProgrami.FindAsync(id);
            if (studijskiProgram != null)
            {
                _context.StudijskiProgrami.Remove(studijskiProgram);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudijskiProgramExists(long id)
        {
            return _context.StudijskiProgrami.Any(e => e.Id == id);
        }
    }
}