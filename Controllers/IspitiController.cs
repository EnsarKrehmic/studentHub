using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<IdentityUser> _userManager;

        public IspitiController(ApplicationDbContext context, ILogger<IspitiController> logger, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string sortOrder)
        {
            var studijskiProgrami = await _context.StudijskiProgrami.ToListAsync();
            var nastavniPlanovi = await _context.NastavniPlanovi.ToListAsync();
            var predmeti = await _context.Predmeti.ToListAsync();
            var ispiti = await _context.Ispiti.ToListAsync();

            var userId = _userManager.GetUserId(User);

            // Filtriranje po ulozi
            if (User.IsInRole("Studentska služba"))
            {
                // Studentska služba vidi sve ispite
                return View(await CreateViewModel(studijskiProgrami, nastavniPlanovi, predmeti, ispiti, sortOrder));
            }

            if (User.IsInRole("Profesor"))
            {
                // Profesori vide samo svoje predmete
                var predmetiIds = await _context.PredmetProfesori
                    .Where(pp => pp.Profesor.AspNetUserId == userId)
                    .Select(pp => pp.PredmetId)
                    .ToListAsync();

                predmeti = predmeti.Where(p => predmetiIds.Contains(p.Id)).ToList();
                ispiti = ispiti.Where(i => predmetiIds.Contains(i.PredmetId)).ToList();
            }

            if (User.IsInRole("Asistent"))
            {
                // Asistenti vide samo svoje predmete
                var predmetiIds = await _context.PredmetAsistenti
                    .Where(pa => pa.Asistent.AspNetUserId == userId)
                    .Select(pa => pa.PredmetId)
                    .ToListAsync();

                predmeti = predmeti.Where(p => predmetiIds.Contains(p.Id)).ToList();
                ispiti = ispiti.Where(i => predmetiIds.Contains(i.PredmetId)).ToList();
            }

            if (User.IsInRole("Student"))
            {
                // Studenti vide samo svoje predmete
                var predmetiIds = await _context.StudentiNaPredmetima
                    .Where(snp => snp.Student.AspNetUserId == userId)
                    .Select(snp => snp.PredmetId)
                    .ToListAsync();

                predmeti = predmeti.Where(p => predmetiIds.Contains(p.Id)).ToList();
                ispiti = ispiti.Where(i => predmetiIds.Contains(i.PredmetId)).ToList();
            }

            return View(await CreateViewModel(studijskiProgrami, nastavniPlanovi, predmeti, ispiti, sortOrder));
        }

        private async Task<List<IspitDetailsViewModel>> CreateViewModel(
            List<StudijskiProgram> studijskiProgrami,
            List<NastavniPlan> nastavniPlanovi,
            List<Predmet> predmeti,
            List<Ispit> ispiti,
            string sortOrder)
        {
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
                DateSortParm = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "",
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

            return viewModel;
        }

        [HttpGet("Details/{id:long}")]
        [Authorize(Roles = "Student, Studentska služba, Profesor, Asistent")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ispit = await _context.Ispiti
                .Include(i => i.Predmet)
                .Include(i => i.StudijskiProgram)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ispit == null)
            {
                return NotFound();
            }

            // Provjera da li je trenutni korisnik student i da li je prijavljen na ovaj ispit
            var userId = _userManager.GetUserId(User);
            var student = await _context.Studenti.FirstOrDefaultAsync(s => s.AspNetUserId == userId);
            bool prijavljen = student != null && await _context.Prijave.AnyAsync(p => p.StudentId == student.Id && p.IspitId == ispit.Id);

            var viewModel = new IspitDetailsViewModel
            {
                IspitId = ispit.Id,
                StudijskiProgram = ispit.StudijskiProgram,
                Predmeti = new List<PredmetIspitViewModel>
            {
            new PredmetIspitViewModel
            {
                Predmet = ispit.Predmet,
                Ispiti = new List<Ispit> { ispit }
            }
            },
                Prijavljen = prijavljen
            };

            return View(viewModel);
        }

        // GET: Ispiti/Create
        [HttpGet("Create")]
        [Authorize(Roles = "Studentska služba, Profesor, Asistent")]
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
                _logger.LogError(ex, "Nastala je greška prilikom učitavanja forme za kreiranje ispita..");
                return View("Error");
            }
        }

        // POST: Ispiti/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba, Profesor, Asistent")]
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

        [HttpGet("Edit/{id:long}")]
        [Authorize(Roles = "Studentska služba, Profesor, Asistent")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ispit = await _context.Ispiti
                .Include(i => i.StudijskiProgram)
                .Include(i => i.NastavniPlan)
                .Include(i => i.Predmet)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ispit == null)
            {
                return NotFound();
            }

            var model = new IspitCreateViewModel
            {
                StudijskiProgramId = ispit.StudijskiProgramId,
                NastavniPlanId = ispit.NastavniPlanId,
                PredmetId = ispit.PredmetId,
                DatumOdrzavanja = ispit.DatumOdrzavanja,
                Lokacija = ispit.Lokacija,
                BrojBodova = ispit.BrojBodova
            };

            ViewBag.StudijskiProgramId = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", ispit.StudijskiProgramId);
            ViewBag.NastavniPlanId = new SelectList(_context.NastavniPlanovi.Where(np => np.StudijskiProgramId == ispit.StudijskiProgramId), "Id", "GodinaStudija", ispit.NastavniPlanId);
            ViewBag.PredmetId = new SelectList(_context.Predmeti.Where(p => p.NastavniPlanId == ispit.NastavniPlanId), "Id", "Naziv", ispit.PredmetId);

            return View(model);
        }

        // POST: Ispiti/Edit/{id}
        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba, Profesor, Asistent")]
        public async Task<IActionResult> Edit(long id, IspitCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.StudijskiProgramId = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", model.StudijskiProgramId);
                ViewBag.NastavniPlanId = new SelectList(_context.NastavniPlanovi.Where(np => np.StudijskiProgramId == model.StudijskiProgramId), "Id", "GodinaStudija", model.NastavniPlanId);
                ViewBag.PredmetId = new SelectList(_context.Predmeti.Where(p => p.NastavniPlanId == model.NastavniPlanId), "Id", "Naziv", model.PredmetId);
                return View(model);
            }

            var ispit = await _context.Ispiti.FindAsync(id);
            if (ispit == null)
            {
                return NotFound();
            }

            ispit.StudijskiProgramId = model.StudijskiProgramId;
            ispit.NastavniPlanId = model.NastavniPlanId;
            ispit.PredmetId = model.PredmetId;
            ispit.DatumOdrzavanja = model.DatumOdrzavanja;
            ispit.Lokacija = model.Lokacija;
            ispit.BrojBodova = model.BrojBodova;

            _context.Update(ispit);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Ispiti/Delete/{id}
        [HttpGet("Delete/{id:long}")]
        [Authorize(Roles = "Studentska služba, Profesor, Asistent")]
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
        [Authorize(Roles = "Studentska služba, Profesor, Asistent")]
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

        [HttpPost("Prijavi/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Prijavi(long id)
        {
            var ispit = await _context.Ispiti
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ispit == null)
            {
                return NotFound("Ispit ne postoji.");
            }

            if (ispit.DatumOdrzavanja.AddDays(-3) <= DateTime.Now)
            {
                return BadRequest("Rok za prijavu ispita je istekao.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _context.Studenti.FirstOrDefaultAsync(s => s.AspNetUserId == userId);

            if (student == null)
            {
                return BadRequest("Ne možete se prijaviti na ispit jer niste registrovani kao student.");
            }

            bool alreadyRegistered = await _context.Prijave
                .AnyAsync(p => p.StudentId == student.Id && p.IspitId == id);

            if (alreadyRegistered)
            {
                return BadRequest("Već ste prijavljeni na ovaj ispit.");
            }

            var prijava = new Prijava
            {
                IspitId = id,
                StudentId = student.Id,
                DatumPrijave = DateTime.Now
            };

            _context.Prijave.Add(prijava);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Uspješno ste prijavili ispit.";

            return RedirectToAction("Details", new { id });
        }

        [HttpPost("Odjavi/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Odjavi(long id)
        {
            var ispit = await _context.Ispiti
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ispit == null)
            {
                return NotFound("Ispit ne postoji.");
            }

            if (ispit.DatumOdrzavanja.AddDays(-2) <= DateTime.Now)
            {
                return BadRequest("Rok za odjavu ispita je istekao.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _context.Studenti.FirstOrDefaultAsync(s => s.AspNetUserId == userId);

            if (student == null)
            {
                return BadRequest("Ne možete se odjaviti jer niste registrovani kao student.");
            }

            var prijava = await _context.Prijave
                .FirstOrDefaultAsync(p => p.StudentId == student.Id && p.IspitId == id);

            if (prijava == null)
            {
                return NotFound("Niste prijavljeni na ovaj ispit.");
            }

            _context.Prijave.Remove(prijava);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Uspješno ste odjavili ispit.";

            return RedirectToAction("Details", new { id });
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

        private bool UserBelongsToStudijskiProgramAndPredmet(long studentId, long? studijskiProgramId, long? predmetId)
        {
            var student = _context.Studenti
                .Include(s => s.StudentStudijskiProgrami)
                .ThenInclude(ssp => ssp.StudijskiProgram)
                .FirstOrDefault(s => s.Id == studentId);

            if (student == null || studijskiProgramId == null || predmetId == null)
            {
                return false;
            }

            bool belongsToStudijskiProgram = student.StudentStudijskiProgrami
                .Any(ssp => ssp.StudijskiProgramId == studijskiProgramId);

            return belongsToStudijskiProgram && student.IsEnrolledInPredmet(predmetId.Value, _context);
        }
    }
}
