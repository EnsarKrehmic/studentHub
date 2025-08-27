using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    [Route("Profesori")]
    public class ProfesoriController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProfesoriController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Profesori
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

            var profesoriQuery = _context.Profesori.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                profesoriQuery = profesoriQuery.Where(p => p.Ime.Contains(searchString) || p.Prezime.Contains(searchString));
            }

            if (studijskiProgramId.HasValue)
            {
                profesoriQuery = profesoriQuery.Where(p => _context.ProfesorStudijskiProgrami.Any(psp => psp.ProfesorId == p.Id && psp.StudijskiProgramId == studijskiProgramId.Value));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    profesoriQuery = profesoriQuery.OrderByDescending(p => p.Ime);
                    break;
                case "surname_asc":
                    profesoriQuery = profesoriQuery.OrderBy(p => p.Prezime);
                    break;
                case "surname_desc":
                    profesoriQuery = profesoriQuery.OrderByDescending(p => p.Prezime);
                    break;
                case "jmbg_asc":
                    profesoriQuery = profesoriQuery.OrderBy(p => p.JMBG);
                    break;
                case "jmbg_desc":
                    profesoriQuery = profesoriQuery.OrderByDescending(p => p.JMBG);
                    break;
                case "email_asc":
                    profesoriQuery = profesoriQuery.OrderBy(p => p.Email);
                    break;
                case "email_desc":
                    profesoriQuery = profesoriQuery.OrderByDescending(p => p.Email);
                    break;
                case "titula_asc":
                    profesoriQuery = profesoriQuery.OrderBy(p => p.ProfesorTitula);
                    break;
                case "titula_desc":
                    profesoriQuery = profesoriQuery.OrderByDescending(p => p.ProfesorTitula);
                    break;
                default:
                    profesoriQuery = profesoriQuery.OrderBy(p => p.Ime);
                    break;
            }

            var profesori = await profesoriQuery.ToListAsync();

            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", studijskiProgramId);

            return View(profesori);
        }

        // GET: Profesori/Details/{id}
        [HttpGet("Details/{id:long}")]
        [Authorize(Roles = "Student, Studentska služba, Profesor, Asistent")]
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

            var studijskiProgrami = await _context.ProfesorStudijskiProgrami
                .Where(psp => psp.ProfesorId == id)
                .Select(psp => psp.StudijskiProgram)
                .ToListAsync();

            // Dohvatanje predmeta koje profesor predaje
            var predmeti = await _context.PredmetProfesori
                .Where(pp => pp.ProfesorId == id)
                .Select(pp => pp.Predmet)
                .ToListAsync();

            var viewModel = new ProfesorDetailsViewModel
            {
                Profesor = profesor,
                StudijskiProgrami = studijskiProgrami,
                Predmeti = predmeti
            };

            return View(viewModel);
        }

        // GET: Profesori/Create
        [HttpGet("Create")]
        [Authorize(Roles = "Studentska služba")]
        public IActionResult Create()
        {
            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv");
            ViewBag.Predmeti = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
            return View();
        }

        // POST: Profesori/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Create(ProfesorCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.StudijskiProgrami = new SelectList(
                    _context.StudijskiProgrami, "Id", "Naziv", model.StudijskiProgramIds);

                IEnumerable<SelectListItem> predmetiZaPrograme;
                if (model.StudijskiProgramIds != null && model.StudijskiProgramIds.Any())
                {
                    predmetiZaPrograme = await _context.Predmeti
                        .Where(p => model.StudijskiProgramIds.Contains(p.NastavniPlan.StudijskiProgramId))
                        .OrderBy(p => p.Naziv)
                        .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Naziv })
                        .ToListAsync();
                }
                else
                {
                    predmetiZaPrograme = Enumerable.Empty<SelectListItem>();
                }

                var selectedPredmeti = (model.PredmetIds ?? new List<long>())
                    .Select(id => id.ToString())
                    .ToList();

                ViewBag.Predmeti = new SelectList(predmetiZaPrograme, "Value", "Text", selectedPredmeti);
                return View(model);
            }

            // 1) Identity
            var identityUser = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(identityUser, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", model.StudijskiProgramIds);
                ViewBag.Predmeti = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
                return View(model);
            }

            // 2) Rola
            try { await _userManager.AddToRoleAsync(identityUser, "Profesor"); } catch { }

            // 3) Profesor
            var profesor = new Profesor
            {
                AspNetUserId = identityUser.Id,
                JMBG = model.JMBG,
                Ime = model.Ime,
                Prezime = model.Prezime,
                Email = model.Email,
                ProfesorTitula = model.ProfesorTitula,
                Uloga = Uloga.Profesor
            };
            _context.Profesori.Add(profesor);
            await _context.SaveChangesAsync();

            // 4) SP veze
            if (model.StudijskiProgramIds != null && model.StudijskiProgramIds.Any())
            {
                foreach (var spId in model.StudijskiProgramIds.Distinct())
                {
                    _context.ProfesorStudijskiProgrami.Add(new ProfesorStudijskiProgram
                    {
                        ProfesorId = profesor.Id,
                        StudijskiProgramId = spId
                    });
                }
            }

            // 5) Predmeti: presijeci po SP
            var validPredmetiIds = (model.StudijskiProgramIds != null && model.StudijskiProgramIds.Any())
                ? await _context.Predmeti
                    .Where(p => model.StudijskiProgramIds.Contains(p.NastavniPlan.StudijskiProgramId))
                    .Select(p => p.Id)
                    .ToListAsync()
                : new List<long>();

            var validSet = new HashSet<long>(validPredmetiIds);
            var predmetIdsZaUpis = (model.PredmetIds ?? new List<long>())
                .Where(id => validSet.Contains(id))
                .Distinct()
                .ToList();

            foreach (var pid in predmetIdsZaUpis)
            {
                _context.PredmetProfesori.Add(new PredmetProfesor
                {
                    ProfesorId = profesor.Id,
                    PredmetId = pid,
                    AspNetUserId = identityUser.Id
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Profesori/Edit/{id}
        [HttpGet("Edit/{id:long}")]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Edit(long id)
        {
            var profesor = await _context.Profesori.FindAsync(id);
            if (profesor == null) return NotFound();

            // Identity e-mail izvor istine
            IdentityUser? identityUser = null;
            if (!string.IsNullOrWhiteSpace(profesor.AspNetUserId))
                identityUser = await _userManager.FindByIdAsync(profesor.AspNetUserId);

            var model = new ProfesorEditViewModel
            {
                Id = profesor.Id,
                Ime = profesor.Ime,
                Prezime = profesor.Prezime,
                JMBG = profesor.JMBG,
                Email = identityUser?.Email ?? profesor.Email,
                ProfesorTitula = profesor.ProfesorTitula,
                Uloga = profesor.Uloga,
                StudijskiProgramIds = await _context.ProfesorStudijskiProgrami
                    .Where(psp => psp.ProfesorId == profesor.Id)
                    .Select(psp => psp.StudijskiProgramId)
                    .ToListAsync(),
                PredmetIds = await _context.PredmetProfesori
                    .Where(pp => pp.ProfesorId == profesor.Id)
                    .Select(pp => pp.PredmetId)
                    .ToListAsync(),

                NewPassword = ProfesorEditViewModel.PasswordSentinel,
                ConfirmNewPassword = ProfesorEditViewModel.PasswordSentinel,
                ChangePassword = false
            };

            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", model.StudijskiProgramIds);

            // Predmeti = svi iz odabranih programa ∪ već dodijeljeni (da ostanu vidljivi)
            var predmetiPlan = await _context.Predmeti
                .Where(p => model.StudijskiProgramIds.Contains(p.NastavniPlan.StudijskiProgramId))
                .Select(p => new { p.Id, p.Naziv })
                .ToListAsync();

            var predmetiUpisani = await _context.Predmeti
                .Where(p => model.PredmetIds.Contains(p.Id))
                .Select(p => new { p.Id, p.Naziv })
                .ToListAsync();

            var predmetiUnion = predmetiPlan
                .Concat(predmetiUpisani)
                .GroupBy(x => x.Id)
                .Select(g => g.First())
                .OrderBy(x => x.Naziv)
                .ToList();

            ViewBag.Predmeti = new SelectList(predmetiUnion, "Id", "Naziv", model.PredmetIds);
            ViewBag.Uloge = new SelectList(Enum.GetValues(typeof(Uloga)).Cast<Uloga>(), model.Uloga);

            return View(model);
        }

        // POST: Profesori/Edit/{id}
        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Edit(long id, ProfesorEditViewModel model)
        {
            if (id != model.Id) return NotFound();

            async Task PopulateForReturnAsync()
            {
                ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", model.StudijskiProgramIds);

                var predmetiPlan = await _context.Predmeti
                    .Where(p => model.StudijskiProgramIds.Contains(p.NastavniPlan.StudijskiProgramId))
                    .Select(p => new { p.Id, p.Naziv })
                    .ToListAsync();

                var predmetiUpisani = await _context.Predmeti
                    .Where(p => (model.PredmetIds ?? new List<long>()).Contains(p.Id))
                    .Select(p => new { p.Id, p.Naziv })
                    .ToListAsync();

                var predmetiUnion = predmetiPlan
                    .Concat(predmetiUpisani)
                    .GroupBy(x => x.Id)
                    .Select(g => g.First())
                    .OrderBy(x => x.Naziv)
                    .ToList();

                ViewBag.Predmeti = new SelectList(predmetiUnion, "Id", "Naziv", model.PredmetIds);
                ViewBag.Uloge = new SelectList(Enum.GetValues(typeof(Uloga)).Cast<Uloga>(), model.Uloga);
            }

            if (!ModelState.IsValid)
            {
                await PopulateForReturnAsync();
                return View(model);
            }

            var profesor = await _context.Profesori
                .Include(p => p.ProfesorStudijskiProgrami)
                .Include(p => p.PredmetProfesori)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (profesor == null) return NotFound();

            // Ažuriranje poslovnih podataka
            profesor.Ime = model.Ime;
            profesor.Prezime = model.Prezime;
            profesor.JMBG = model.JMBG;
            profesor.Email = model.Email;
            profesor.ProfesorTitula = model.ProfesorTitula;
            profesor.Uloga = model.Uloga;

            // Identity e-mail + lozinka
            if (!string.IsNullOrWhiteSpace(profesor.AspNetUserId))
            {
                var identityUser = await _userManager.FindByIdAsync(profesor.AspNetUserId);
                if (identityUser != null)
                {
                    if (!string.Equals(identityUser.Email, model.Email, StringComparison.OrdinalIgnoreCase))
                    {
                        identityUser.Email = model.Email;
                        identityUser.UserName = model.Email;
                        var updateRes = await _userManager.UpdateAsync(identityUser);
                        if (!updateRes.Succeeded)
                        {
                            foreach (var e in updateRes.Errors)
                                ModelState.AddModelError(nameof(model.Email), e.Description);
                            await PopulateForReturnAsync();
                            return View(model);
                        }
                    }

                    var wantsPasswordChange = model.ChangePassword
                                               && !string.IsNullOrWhiteSpace(model.NewPassword)
                                               && model.NewPassword != ProfesorEditViewModel.PasswordSentinel;

                    if (wantsPasswordChange)
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(identityUser);
                        var passRes = await _userManager.ResetPasswordAsync(identityUser, token, model.NewPassword!);
                        if (!passRes.Succeeded)
                        {
                            foreach (var e in passRes.Errors)
                                ModelState.AddModelError(nameof(model.NewPassword), e.Description);
                            await PopulateForReturnAsync();
                            return View(model);
                        }
                    }
                }
            }

            // SP veze: zamijeni set
            var stariSP = await _context.ProfesorStudijskiProgrami
                .Where(psp => psp.ProfesorId == profesor.Id)
                .ToListAsync();
            _context.ProfesorStudijskiProgrami.RemoveRange(stariSP);

            if (model.StudijskiProgramIds != null && model.StudijskiProgramIds.Any())
            {
                foreach (var spId in model.StudijskiProgramIds.Distinct())
                {
                    _context.ProfesorStudijskiProgrami.Add(new ProfesorStudijskiProgram
                    {
                        ProfesorId = profesor.Id,
                        StudijskiProgramId = spId
                    });
                }
            }

            // Predmeti: dozvoli samo one iz odabranih SP
            var validPredmetiIds = (model.StudijskiProgramIds != null && model.StudijskiProgramIds.Any())
                ? await _context.Predmeti
                    .Where(p => model.StudijskiProgramIds.Contains(p.NastavniPlan.StudijskiProgramId))
                    .Select(p => p.Id)
                    .ToListAsync()
                : new List<long>();

            var validSet = new HashSet<long>(validPredmetiIds);
            var predmetIdsZaUpis = (model.PredmetIds ?? new List<long>())
                .Where(id => validSet.Contains(id))
                .Distinct()
                .ToList();

            // zamijeni set
            var stariPP = await _context.PredmetProfesori
                .Where(pp => pp.ProfesorId == profesor.Id)
                .ToListAsync();
            _context.PredmetProfesori.RemoveRange(stariPP);

            foreach (var pid in predmetIdsZaUpis)
            {
                _context.PredmetProfesori.Add(new PredmetProfesor
                {
                    ProfesorId = profesor.Id,
                    PredmetId = pid,
                    AspNetUserId = profesor.AspNetUserId
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Profesori/Delete/{id}
        [HttpGet("Delete/{id:long}")]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profesor = await _context.Profesori.FirstOrDefaultAsync(m => m.Id == id);
            if (profesor == null)
            {
                return NotFound();
            }

            return View(profesor);
        }

        // POST: Profesori/Delete/{id}
        [HttpPost("Delete/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var profesor = await _context.Profesori.FindAsync(id);
            if (profesor != null)
            {
                _context.Profesori.Remove(profesor);
                var korisnik = await _context.Korisnici.FindAsync(id);
                if (korisnik != null)
                {
                    _context.Korisnici.Remove(korisnik);
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: /Profesori/GetPredmetiByStudijskiProgram
        [HttpPost("GetPredmetiByStudijskiProgram")]
        [Authorize(Roles = "Studentska služba")]
        // [IgnoreAntiforgeryToken] // ako Cors/CSRF nije podešen za fetch, po potrebi otkomentiraj
        public async Task<IActionResult> GetPredmetiByStudijskiProgram([FromBody] List<long> studijskiProgramIds)
        {
            if (studijskiProgramIds == null || !studijskiProgramIds.Any())
                return Json(Array.Empty<object>());

            var predmeti = await _context.Predmeti
                .Where(p => studijskiProgramIds.Contains(p.NastavniPlan.StudijskiProgramId))
                .OrderBy(p => p.Naziv)
                .Select(p => new { id = p.Id, naziv = p.Naziv })
                .ToListAsync();

            return Json(predmeti);
        }
    }
}