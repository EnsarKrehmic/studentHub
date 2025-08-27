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
    [Route("Asistenti")]
    public class AsistentiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AsistentiController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

        // GET: Asistenti/Create
        [HttpGet("Create")]
        [Authorize(Roles = "Studentska služba")]
        public IActionResult Create()
        {
            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv");
            ViewBag.Predmeti = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text"); // inicijalno prazno
            return View();
        }

        // POST: Asistenti/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Create(AsistentCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", model.StudijskiProgramIds);

                // koristimo SelectListItem da izbjegnemo probleme sa tipovima
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

                ViewBag.Predmeti = new SelectList(predmetiZaPrograme, "Value", "Text", model.PredmetIds?.Select(x => x.ToString()));
                return View(model);
            }

            // 1) Identity user
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
            try { await _userManager.AddToRoleAsync(identityUser, "Asistent"); } catch { }

            // 3) Asistent entitet
            var asistent = new Asistent
            {
                AspNetUserId = identityUser.Id,
                JMBG = model.JMBG,
                Ime = model.Ime,
                Prezime = model.Prezime,
                Email = model.Email,
                AsistentTitula = model.AsistentTitula,
                Uloga = Uloga.Asistent
            };
            _context.Asistenti.Add(asistent);
            await _context.SaveChangesAsync();

            // 4) Veze SP
            if (model.StudijskiProgramIds != null && model.StudijskiProgramIds.Any())
            {
                foreach (var spId in model.StudijskiProgramIds.Distinct())
                {
                    _context.AsistentStudijskiProgrami.Add(new AsistentStudijskiProgram
                    {
                        AsistentId = asistent.Id,
                        StudijskiProgramId = spId
                    });
                }
            }

            // 5) Predmeti (validni samo u okviru izabranih SP)
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
                _context.PredmetAsistenti.Add(new PredmetAsistent
                {
                    AsistentId = asistent.Id,
                    PredmetId = pid,
                    AspNetUserId = identityUser.Id
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Asistenti/Edit/{id}
        [HttpGet("Edit/{id:long}")]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Edit(long id)
        {
            var asistent = await _context.Asistenti.FindAsync(id);
            if (asistent == null) return NotFound();

            // Identity e-mail kao izvor istine
            IdentityUser? identityUser = null;
            if (!string.IsNullOrWhiteSpace(asistent.AspNetUserId))
                identityUser = await _userManager.FindByIdAsync(asistent.AspNetUserId);

            var model = new AsistentEditViewModel
            {
                Id = asistent.Id,
                Ime = asistent.Ime,
                Prezime = asistent.Prezime,
                JMBG = asistent.JMBG,
                Email = identityUser?.Email ?? asistent.Email,
                AsistentTitula = asistent.AsistentTitula,
                Uloga = asistent.Uloga,
                StudijskiProgramIds = await _context.AsistentStudijskiProgrami
                    .Where(x => x.AsistentId == asistent.Id)
                    .Select(x => x.StudijskiProgramId)
                    .ToListAsync(),
                PredmetIds = await _context.PredmetAsistenti
                    .Where(x => x.AsistentId == asistent.Id)
                    .Select(x => x.PredmetId)
                    .ToListAsync(),

                NewPassword = AsistentEditViewModel.PasswordSentinel,
                ConfirmNewPassword = AsistentEditViewModel.PasswordSentinel,
                ChangePassword = false
            };

            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", model.StudijskiProgramIds);

            // Predmeti = (iz odabranih SP) ∪ (već dodijeljeni)
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

        // POST: Asistenti/Edit/{id}
        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Edit(long id, AsistentEditViewModel model)
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

            var asistent = await _context.Asistenti.FindAsync(id);
            if (asistent == null) return NotFound();

            // poslovni podaci
            asistent.Ime = model.Ime;
            asistent.Prezime = model.Prezime;
            asistent.JMBG = model.JMBG;
            asistent.Email = model.Email;
            asistent.AsistentTitula = model.AsistentTitula;
            asistent.Uloga = model.Uloga;

            // Identity e-mail + (opciono) lozinka
            if (!string.IsNullOrWhiteSpace(asistent.AspNetUserId))
            {
                var identityUser = await _userManager.FindByIdAsync(asistent.AspNetUserId);
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
                                               && model.NewPassword != AsistentEditViewModel.PasswordSentinel;

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

            // Zamijeni veze SP
            var stariSP = await _context.AsistentStudijskiProgrami
                .Where(x => x.AsistentId == asistent.Id)
                .ToListAsync();
            _context.AsistentStudijskiProgrami.RemoveRange(stariSP);

            if (model.StudijskiProgramIds != null && model.StudijskiProgramIds.Any())
            {
                foreach (var spId in model.StudijskiProgramIds.Distinct())
                {
                    _context.AsistentStudijskiProgrami.Add(new AsistentStudijskiProgram
                    {
                        AsistentId = asistent.Id,
                        StudijskiProgramId = spId
                    });
                }
            }

            // Validni predmeti po izabranim SP
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

            // Zamijeni veze predmeta
            var stariPA = await _context.PredmetAsistenti
                .Where(x => x.AsistentId == asistent.Id)
                .ToListAsync();
            _context.PredmetAsistenti.RemoveRange(stariPA);

            foreach (var pid in predmetIdsZaUpis)
            {
                _context.PredmetAsistenti.Add(new PredmetAsistent
                {
                    AsistentId = asistent.Id,
                    PredmetId = pid,
                    AspNetUserId = asistent.AspNetUserId
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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

        // POST: /Asistenti/GetPredmetiByStudijskiProgram
        [HttpPost("GetPredmetiByStudijskiProgram")]
        [Authorize(Roles = "Studentska služba")]
        // [IgnoreAntiforgeryToken] // po potrebi otkomentiraj ako fetch bez AntiForgery
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