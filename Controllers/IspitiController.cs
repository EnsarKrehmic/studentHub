using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;

namespace StudentHub.Controllers
{
    [Route("Ispiti")]
    public class IspitiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IspitiController> _logger;

        public IspitiController(ApplicationDbContext context, ILogger<IspitiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Ispiti
        [HttpGet("")]
        public async Task<IActionResult> Index(string sortOrder)
        {
            var studijskiProgrami = await _context.StudijskiProgrami.ToListAsync();
            var nastavniPlanovi = await _context.NastavniPlanovi.ToListAsync();
            var predmeti = await _context.Predmeti.ToListAsync();
            var ispiti = await _context.Ispiti.ToListAsync();

            var viewModel = studijskiProgrami.Select(sp => new IspitDetailsViewModel
            {
                StudijskiProgram = sp,
                NastavniPlanovi = nastavniPlanovi
                    .Where(np => np.StudijskiProgramId == sp.Id && ispiti.Any(i => i.NastavniPlanId == np.Id))
                    .Select(np => new NastavniPlanIspitViewModel
                    {
                        NastavniPlan = np,
                        Predmeti = predmeti
                            .Where(p => p.NastavniPlanId == np.Id)
                            .Select(p => new PredmetIspitViewModel
                            {
                                Predmet = p,
                                Ispiti = ispiti
                                    .Where(i => i.PredmetId == p.Id)
                                    .ToList()
                            })
                            .ToList()
                    })
                    .ToList(),
                CurrentSort = sortOrder,
                DateSortParm = String.IsNullOrEmpty(sortOrder) ? "date_desc" : "",
                LocationSortParm = sortOrder == "Location" ? "location_desc" : "Location",
                PointsSortParm = sortOrder == "Points" ? "points_desc" : "Points"
            }).ToList();

            // Sortiranje ispita
            foreach (var program in viewModel)
            {
                foreach (var plan in program.NastavniPlanovi)
                {
                    foreach (var predmet in plan.Predmeti)
                    {
                        predmet.Ispiti = predmet.Ispiti switch
                        {
                            var list when sortOrder == "date_desc" => list.OrderByDescending(i => i.DatumOdrzavanja).ToList(),
                            var list when sortOrder == "Location" => list.OrderBy(i => i.Lokacija).ToList(),
                            var list when sortOrder == "location_desc" => list.OrderByDescending(i => i.Lokacija).ToList(),
                            var list when sortOrder == "Points" => list.OrderBy(i => i.BrojBodova).ToList(),
                            var list when sortOrder == "points_desc" => list.OrderByDescending(i => i.BrojBodova).ToList(),
                            _ => predmet.Ispiti.OrderBy(i => i.DatumOdrzavanja).ToList()
                        };
                    }
                }
            }

            return View(viewModel);
        }

        // GET: Ispiti/Details/{id}
        [HttpGet("Details/{id:long}")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ispit = await _context.Ispiti
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ispit == null) return NotFound();

            return View(ispit);
        }

        // GET: Ispiti/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            try
            {
                ViewBag.StudijskiProgramId = _context.StudijskiProgrami
                    .Select(sp => new SelectListItem
                    {
                        Value = sp.Id.ToString(),
                        Text = $"Studijski program: {sp.Naziv}."
                    }).ToList();
                ViewBag.NastavniPlanId = _context.NastavniPlanovi
                    .Include(np => np.StudijskiProgram)
                    .Select(np => new SelectListItem
                    {
                        Value = np.Id.ToString(),
                        Text = $"Nastavni plan za {np.StudijskiProgram.Naziv}: {np.GodinaStudija}. godina"
                    }).ToList();
                ViewBag.PredmetId = _context.Predmeti
                    .Include(p => p.NastavniPlan)
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = $"Predmet: {p.Naziv}: ({p.NastavniPlan.StudijskiProgram.Naziv})"
                    }).ToList();
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Nastala je greška prilikom kreiranja Create forme..");
                return View("Error");
            }
        }

        // POST: Ispiti/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IspitCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Count > 0)
                    {
                        _logger.LogWarning("Missing or invalid attribute: {Key}", state.Key);
                    }
                }

                // Repopulate dropdowns
                ViewBag.StudijskiProgramId = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", model.StudijskiProgramId);
                ViewBag.NastavniPlanId = new SelectList(
                    _context.NastavniPlanovi.Where(np => np.StudijskiProgramId == model.StudijskiProgramId),
                    "Id",
                    "GodinaStudija",
                    model.NastavniPlanId
                );
                ViewBag.PredmetId = new SelectList(
                    _context.Predmeti.Where(p => p.NastavniPlanId == model.NastavniPlanId),
                    "Id",
                    "Naziv",
                    model.PredmetId
                );
                return View(model);
            }

            var ispit = new Ispit
            {
                StudijskiProgramId = model.StudijskiProgramId,
                NastavniPlanId = model.NastavniPlanId,
                PredmetId = model.PredmetId,
                DatumOdrzavanja = model.DatumOdrzavanja,
                Lokacija = model.Lokacija,
                BrojBodova = model.BrojBodova
            };

            _context.Add(ispit);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Ispiti/Edit/{id}
        [HttpGet("Edit/{id:long}")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var ispit = await _context.Ispiti.FindAsync(id);
                if (ispit == null)
                {
                    return NotFound();
                }

                ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", ispit.StudijskiProgramId);
                ViewBag.NastavniPlanovi = new SelectList(_context.NastavniPlanovi.Where(np => np.StudijskiProgramId == ispit.StudijskiProgramId), "Id", "GodinaStudija", ispit.NastavniPlanId);
                ViewBag.Predmeti = new SelectList(_context.Predmeti.Where(p => p.NastavniPlanId == ispit.NastavniPlanId), "Id", "Naziv", ispit.PredmetId);
                return View(ispit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while preparing the Edit view.");
                return View("Error");
            }
        }

        // POST: Ispiti/Edit/{id}
        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,DatumOdrzavanja,Lokacija,BrojBodova,PredmetId,StudijskiProgramId,NastavniPlanId")] Ispit ispit)
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
            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", ispit.StudijskiProgramId);
            ViewBag.NastavniPlanovi = new SelectList(_context.NastavniPlanovi.Where(np => np.StudijskiProgramId == ispit.StudijskiProgramId), "Id", "GodinaStudija", ispit.NastavniPlanId);
            ViewBag.Predmeti = new SelectList(_context.Predmeti.Where(p => p.NastavniPlanId == ispit.NastavniPlanId), "Id", "Naziv", ispit.PredmetId);
            return View(ispit);
        }

        // GET: Ispiti/Delete/{id}
        [HttpGet("Delete/{id:long}")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null) return NotFound();

            var ispit = await _context.Ispiti
                .Include(i => i.Predmet)
                .Include(i => i.StudijskiProgram)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ispit == null) return NotFound();
            return View(ispit);
        }

        // POST: Ispiti/Delete/{id}
        [HttpPost("Delete/{id:long}")]
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

        // POST: Ispiti/Prijavi/{id}
        // POST: Ispiti/Prijavi/{id}
        [HttpPost("Prijavi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Prijavi(long id)
        {
            // Uklonjena provera autentifikacije i tvrdnji

            var ispit = await _context.Ispiti
                .Include(i => i.Predmet)
                .Include(i => i.StudijskiProgram)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ispit == null)
            {
                return NotFound("Ispit ne postoji.");
            }

            // Check if the registration period is valid (3 days before the exam date)
            if (ispit.DatumOdrzavanja.AddDays(-3) <= DateTime.Now)
            {
                return BadRequest("Rok za prijavu ispita je istekao.");
            }

            var prijava = new Prijava
            {
                IspitId = id,
                // Postavite StudentId na neki podrazumevani ID ili ga uklonite ako nije potreban
                StudentId = 0, // Podrazumevani ID za gosta
                DatumPrijave = DateTime.Now
            };

            _context.Prijave.Add(prijava);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool UserBelongsToStudijskiProgramAndPredmet(long studentId, long? studijskiProgramId, long? predmetId)
        {
            var student = _context.Studenti
                .Include(s => s.StudijskiProgram)
                .FirstOrDefault(s => s.Id == studentId);

            if (student == null || studijskiProgramId == null || predmetId == null)
            {
                return false;
            }

            return student.StudijskiProgramId == studijskiProgramId && student.IsEnrolledInPredmet(predmetId.Value, _context);
        }

        [HttpGet("GetNastavniPlanoviByStudijskiProgram/{studijskiProgramId}")]
        public async Task<IActionResult> GetNastavniPlanoviByStudijskiProgram(long studijskiProgramId)
        {
            var nastavniPlanovi = await _context.NastavniPlanovi
                .Where(np => np.StudijskiProgramId == studijskiProgramId)
                .Select(np => new { id = np.Id, godinaStudija = np.GodinaStudija })
                .ToListAsync();

            return Json(nastavniPlanovi);
        }

        [HttpGet("GetPredmetiByNastavniPlan/{nastavniPlanId}")]
        public async Task<IActionResult> GetPredmetiByNastavniPlan(long nastavniPlanId)
        {
            var predmeti = await _context.Predmeti
                .Where(p => p.NastavniPlanId == nastavniPlanId)
                .Select(p => new { id = p.Id, naziv = p.Naziv })
                .ToListAsync();

            return Json(predmeti);
        }
    }
}
