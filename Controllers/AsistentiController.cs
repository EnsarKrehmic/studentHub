using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;
using System.Linq;
using System.Threading.Tasks;

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
        [HttpGet("")]
        public async Task<IActionResult> Index(string sortOrder, string searchString, long? studijskiProgramId)
        {
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["SurnameSortParm"] = sortOrder == "surname_asc" ? "surname_desc" : "surname_asc";
            ViewData["JMBGSortParm"] = sortOrder == "jmbg_asc" ? "jmbg_desc" : "jmbg_asc";
            ViewData["EmailSortParm"] = sortOrder == "email_asc" ? "email_desc" : "email_asc";
            ViewData["TitulaSortParm"] = sortOrder == "titula_asc" ? "titula_desc" : "titula_asc";
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStudijskiProgramId"] = studijskiProgramId;

            var asistentiQuery = _context.Asistenti.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                asistentiQuery = asistentiQuery.Where(p => p.Ime.Contains(searchString) || p.Prezime.Contains(searchString));
            }

            if (studijskiProgramId.HasValue)
            {
                asistentiQuery = asistentiQuery.Where(p => _context.AsistentStudijskiProgrami.Any(psp => psp.AsistentId == p.Id && psp.StudijskiProgramId == studijskiProgramId.Value));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    asistentiQuery = asistentiQuery.OrderByDescending(p => p.Ime);
                    break;
                case "surname_asc":
                    asistentiQuery = asistentiQuery.OrderBy(p => p.Prezime);
                    break;
                case "surname_desc":
                    asistentiQuery = asistentiQuery.OrderByDescending(p => p.Prezime);
                    break;
                case "jmbg_asc":
                    asistentiQuery = asistentiQuery.OrderBy(p => p.JMBG);
                    break;
                case "jmbg_desc":
                    asistentiQuery = asistentiQuery.OrderByDescending(p => p.JMBG);
                    break;
                case "email_asc":
                    asistentiQuery = asistentiQuery.OrderBy(p => p.Email);
                    break;
                case "email_desc":
                    asistentiQuery = asistentiQuery.OrderByDescending(p => p.Email);
                    break;
                case "titula_asc":
                    asistentiQuery = asistentiQuery.OrderBy(p => p.AsistentTitula);
                    break;
                case "titula_desc":
                    asistentiQuery = asistentiQuery.OrderByDescending(p => p.AsistentTitula);
                    break;
                default:
                    asistentiQuery = asistentiQuery.OrderBy(p => p.Ime);
                    break;
            }

            var asistenti = await asistentiQuery.ToListAsync();

            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", studijskiProgramId);

            return View(asistenti);
        }

        // GET: Asistenti/Details/{id}
        [HttpGet("Details/{id:long}")]
        [Authorize(Roles = "Student, Studentska služba, Profesor, Asistent")]
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

            var studijskiProgrami = await _context.AsistentStudijskiProgrami
                .Where(psp => psp.AsistentId == id)
                .Select(psp => psp.StudijskiProgram)
                .ToListAsync();

            // Dohvatanje predmeta koje asistent predaje
            var predmeti = await _context.PredmetAsistenti
                .Where(pa => pa.AsistentId == id)
                .Select(pa => pa.Predmet)
                .ToListAsync();

            var viewModel = new AsistentDetailsViewModel
            {
                Asistent = asistent,
                StudijskiProgrami = studijskiProgrami,
                Predmeti = predmeti
            };

            return View(viewModel);
        }

        // GET: Asistenti/Edit/{id}
        [HttpGet("Edit/{id:long}")]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Edit(long id)
        {
            var asistent = await _context.Asistenti.FindAsync(id);
            if (asistent == null)
            {
                return NotFound();
            }

            var model = new AsistentEditViewModel
            {
                Id = asistent.Id,
                Ime = asistent.Ime,
                Prezime = asistent.Prezime,
                JMBG = asistent.JMBG,
                Email = asistent.Email,
                AsistentTitula = asistent.AsistentTitula,
                Uloga = asistent.Uloga,
                StudijskiProgramIds = await _context.AsistentStudijskiProgrami
                    .Where(psp => psp.AsistentId == asistent.Id)
                    .Select(psp => psp.StudijskiProgramId)
                    .ToListAsync(),
                PredmetIds = await _context.PredmetAsistenti
                    .Where(pp => pp.AsistentId == asistent.Id)
                    .Select(pp => pp.PredmetId)
                    .ToListAsync()
            };

            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv");
            ViewBag.Predmeti = new SelectList(_context.Predmeti, "Id", "Naziv");

            ViewBag.Uloge = Enum.GetValues(typeof(Uloga))
                            .Cast<Uloga>()
                            .Select(u => new SelectListItem
                            {
                                Value = ((int)u).ToString(),
                                Text = u.ToString(),
                                Selected = u == asistent.Uloga
                            }); return View(model);
        }

        // POST: Asistenti/Edit/{id}
        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Edit(long id, AsistentEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var asistent = await _context.Asistenti.FindAsync(id);
                if (asistent == null)
                {
                    return NotFound();
                }

                asistent.Ime = model.Ime;
                asistent.Prezime = model.Prezime;
                asistent.JMBG = model.JMBG;
                asistent.Email = model.Email;
                asistent.AsistentTitula = model.AsistentTitula;
                asistent.Uloga = model.Uloga;

                _context.Update(asistent);

                var existingStudijskiProgrami = await _context.AsistentStudijskiProgrami
                    .Where(psp => psp.AsistentId == asistent.Id)
                    .Select(psp => psp.StudijskiProgramId)
                    .ToListAsync();
                var newStudijskiProgrami = model.StudijskiProgramIds.Except(existingStudijskiProgrami).ToList();
                var removedStudijskiProgrami = existingStudijskiProgrami.Except(model.StudijskiProgramIds).ToList();

                foreach (var studijskiProgramId in newStudijskiProgrami)
                {
                    _context.AsistentStudijskiProgrami.Add(new AsistentStudijskiProgram
                    {
                        AsistentId = asistent.Id,
                        StudijskiProgramId = studijskiProgramId
                    });
                }

                foreach (var studijskiProgramId in removedStudijskiProgrami)
                {
                    var asistentStudijskiProgram = await _context.AsistentStudijskiProgrami
                        .FirstOrDefaultAsync(psp => psp.AsistentId == asistent.Id && psp.StudijskiProgramId == studijskiProgramId);
                    if (asistentStudijskiProgram != null)
                    {
                        _context.AsistentStudijskiProgrami.Remove(asistentStudijskiProgram);
                    }
                }

                var existingPredmeti = await _context.PredmetProfesori
                    .Where(pp => pp.ProfesorId == asistent.Id)
                    .Select(pp => pp.PredmetId)
                    .ToListAsync();
                var newPredmeti = model.PredmetIds.Except(existingPredmeti).ToList();
                var removedPredmeti = existingPredmeti.Except(model.PredmetIds).ToList();

                foreach (var predmetId in newPredmeti)
                {
                    _context.PredmetAsistenti.Add(new PredmetAsistent
                    {
                        AsistentId = asistent.Id,
                        PredmetId = predmetId
                    });
                }

                foreach (var predmetId in removedPredmeti)
                {
                    var predmetAsistent = await _context.PredmetAsistenti
                        .FirstOrDefaultAsync(pp => pp.AsistentId == asistent.Id && pp.PredmetId == predmetId);
                    if (predmetAsistent != null)
                    {
                        _context.PredmetAsistenti.Remove(predmetAsistent);
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv");
            ViewBag.Predmeti = new SelectList(_context.Predmeti, "Id", "Naziv");
            ViewBag.Uloge = new SelectList(Enum.GetValues(typeof(Uloga)).Cast<Uloga>());
            return View(model);
        }

        // GET: Asistenti/Delete/{id}
        [HttpGet("Delete/{id:long}")]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asistent = await _context.Asistenti.FirstOrDefaultAsync(m => m.Id == id);
            if (asistent == null)
            {
                return NotFound();
            }

            return View(asistent);
        }

        // POST: Asistenti/Delete/{id}
        [HttpPost("Delete/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var asistent = await _context.Asistenti.FindAsync(id);
            if (asistent != null)
            {
                // 1. Brisanje povezanih zapisa iz AsistentStudijskiProgram
                var asistentStudijskiProgrami = _context.AsistentStudijskiProgrami
                    .Where(asp => asp.AsistentId == asistent.Id);
                _context.AsistentStudijskiProgrami.RemoveRange(asistentStudijskiProgrami);

                // 2. Brisanje povezanih zapisa iz PredmetAsistenti
                var predmetAsistenti = _context.PredmetAsistenti
                    .Where(pa => pa.AsistentId == asistent.Id);
                _context.PredmetAsistenti.RemoveRange(predmetAsistenti);

                // 3. Brisanje asistenta iz korisnika ako postoji
                var korisnik = await _context.Korisnici.FindAsync(id);
                if (korisnik != null)
                {
                    _context.Korisnici.Remove(korisnik);
                }

                // 4. Konačno brisanje asistenta
                _context.Asistenti.Remove(asistent);

                // 5. Čuvanje promjena u bazi
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("GetPredmetiByStudijskiProgram")]
        public async Task<IActionResult> GetPredmetiByStudijskiProgram([FromBody] List<long> studijskiProgramIds)
        {
            if (studijskiProgramIds == null || !studijskiProgramIds.Any())
            {
                return Json(new List<object>());
            }

            var predmeti = await _context.Predmeti
                .Where(p => studijskiProgramIds.Contains(p.NastavniPlan.StudijskiProgramId))
                .Select(p => new { id = p.Id, naziv = p.Naziv })
                .ToListAsync();

            return Json(predmeti);
        }
    }
}