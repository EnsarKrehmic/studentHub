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
    [Route("StudentskaSluzba")]
    public class StudentskaSluzbaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public StudentskaSluzbaController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: StudentskaSluzba
        [HttpGet("")]
        public async Task<IActionResult> Index(string sortOrder, string searchString, long? studijskiProgramId)
        {
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["SurnameSortParm"] = sortOrder == "surname_asc" ? "surname_desc" : "surname_asc";
            ViewData["JMBGSortParm"] = sortOrder == "jmbg_asc" ? "jmbg_desc" : "jmbg_asc";
            ViewData["EmailSortParm"] = sortOrder == "email_asc" ? "email_desc" : "email_asc";
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStudijskiProgramId"] = studijskiProgramId;

            var studentskesluzbequery = _context.StudentskeSluzbe
                .Include(s => s.StudentskaSluzbaStudijskiProgrami)
                    .ThenInclude(ssp => ssp.StudijskiProgram)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                studentskesluzbequery = studentskesluzbequery.Where(s => s.Ime.Contains(searchString) || s.Prezime.Contains(searchString));
            }

            if (studijskiProgramId.HasValue)
            {
                studentskesluzbequery = studentskesluzbequery.Where(p => _context.StudentskaSluzbaStudijskiProgrami.Any(psp => psp.StudentskaSluzbaId == p.Id && psp.StudijskiProgramId == studijskiProgramId.Value));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    studentskesluzbequery = studentskesluzbequery.OrderByDescending(p => p.Ime);
                    break;
                case "surname_asc":
                    studentskesluzbequery = studentskesluzbequery.OrderBy(p => p.Prezime);
                    break;
                case "surname_desc":
                    studentskesluzbequery = studentskesluzbequery.OrderByDescending(p => p.Prezime);
                    break;
                case "jmbg_asc":
                    studentskesluzbequery = studentskesluzbequery.OrderBy(p => p.JMBG);
                    break;
                case "jmbg_desc":
                    studentskesluzbequery = studentskesluzbequery.OrderByDescending(p => p.JMBG);
                    break;
                case "email_asc":
                    studentskesluzbequery = studentskesluzbequery.OrderBy(p => p.Email);
                    break;
                case "email_desc":
                    studentskesluzbequery = studentskesluzbequery.OrderByDescending(p => p.Email);
                    break;
                default:
                    studentskesluzbequery = studentskesluzbequery.OrderBy(p => p.Ime);
                    break;
            }

            var studentskaSluzba = await studentskesluzbequery.ToListAsync();

            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", studijskiProgramId);

            return View(studentskaSluzba);
        }

        // GET: StudentskaSluzba/Details/{id}
        [HttpGet("Details/{id:long}")]
        [Authorize(Roles = "Student, Studentska služba, Profesor, Asistent")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studentskaSluzba = await _context.StudentskeSluzbe
                .Include(s => s.StudentskaSluzbaStudijskiProgrami)
                    .ThenInclude(ssp => ssp.StudijskiProgram)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (studentskaSluzba == null)
            {
                return NotFound();
            }

            var studijskiProgrami = studentskaSluzba.StudentskaSluzbaStudijskiProgrami
                .Select(ssp => ssp.StudijskiProgram)
                .ToList();

            var viewModel = new StudentskaSluzbaDetailsViewModel
            {
                StudentskaSluzba = studentskaSluzba,
                StudijskiProgrami = studijskiProgrami
            };

            return View(viewModel);
        }

        // GET: StudentskaSluzba/Create
        [HttpGet("Create")]
        [Authorize(Roles = "Studentska služba")]
        public IActionResult Create()
        {
            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv");
            return View();
        }

        // POST: StudentskaSluzba/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Create(StudentskaSluzbaCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", model.StudijskiProgramIds);
                return View(model);
            }

            // 1. Kreiranje Identity User-a
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
                return View(model);
            }

            // 2. Dodavanje u rolu "Studentska služba"
            try { await _userManager.AddToRoleAsync(identityUser, "Studentska služba"); }
            catch { /* log opcionalno */ }

            // 3. Kreiranje SS entiteta
            var studentskaSluzba = new StudentskaSluzba
            {
                AspNetUserId = identityUser.Id,
                JMBG = model.JMBG,
                Ime = model.Ime,
                Prezime = model.Prezime,
                Email = model.Email,
                Uloga = Uloga.StudentskaSluzba
            };

            _context.StudentskeSluzbe.Add(studentskaSluzba);
            await _context.SaveChangesAsync();

            // 4. Veze sa studijskim programima
            if (model.StudijskiProgramIds != null && model.StudijskiProgramIds.Any())
            {
                foreach (var studijskiProgramId in model.StudijskiProgramIds.Distinct())
                {
                    _context.StudentskaSluzbaStudijskiProgrami.Add(new StudentskaSluzbaStudijskiProgram
                    {
                        StudentskaSluzbaId = studentskaSluzba.Id,
                        StudijskiProgramId = studijskiProgramId
                    });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: StudentskaSluzba/Edit/{id}
        [HttpGet("Edit/{id:long}")]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Edit(long id)
        {
            var ss = await _context.StudentskeSluzbe
                .Include(x => x.StudentskaSluzbaStudijskiProgrami)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (ss == null) return NotFound();

            // Identity e-mail kao izvor istine
            IdentityUser? identityUser = null;
            if (!string.IsNullOrWhiteSpace(ss.AspNetUserId))
                identityUser = await _userManager.FindByIdAsync(ss.AspNetUserId);

            var povezaniProgramiIds = ss.StudentskaSluzbaStudijskiProgrami
                .Select(sp => sp.StudijskiProgramId)
                .ToList();

            var model = new StudentskaSluzbaEditViewModel
            {
                Id = ss.Id,
                JMBG = ss.JMBG,
                Ime = ss.Ime,
                Prezime = ss.Prezime,
                Email = identityUser?.Email ?? ss.Email,
                Uloga = ss.Uloga,
                StudijskiProgramiIds = povezaniProgramiIds,

                // Sentinel i toggle
                NewPassword = StudentskaSluzbaEditViewModel.PasswordSentinel,
                ConfirmNewPassword = StudentskaSluzbaEditViewModel.PasswordSentinel,
                ChangePassword = false
            };

            // Preselektuj više programa
            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", model.StudijskiProgramiIds);
            return View(model);
        }

        // POST: StudentskaSluzba/Edit/{id}
        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Edit(long id, StudentskaSluzbaEditViewModel model)
        {
            if (id != model.Id) return NotFound();

            // helper za repop
            void PopulatePrograms()
            {
                ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", model.StudijskiProgramiIds);
            }

            if (!ModelState.IsValid)
            {
                PopulatePrograms();
                return View(model);
            }

            var ss = await _context.StudentskeSluzbe
                .Include(x => x.StudentskaSluzbaStudijskiProgrami)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (ss == null) return NotFound();

            // Ažuriranje osnovnih podataka
            ss.JMBG = model.JMBG;
            ss.Ime = model.Ime;
            ss.Prezime = model.Prezime;
            ss.Email = model.Email;
            ss.Uloga = model.Uloga;

            // Identity e-mail i lozinka
            if (!string.IsNullOrWhiteSpace(ss.AspNetUserId))
            {
                var identityUser = await _userManager.FindByIdAsync(ss.AspNetUserId);
                if (identityUser != null)
                {
                    // E-mail/username promjena
                    if (!string.Equals(identityUser.Email, model.Email, StringComparison.OrdinalIgnoreCase))
                    {
                        identityUser.Email = model.Email;
                        identityUser.UserName = model.Email; // ako koristite e-mail kao username
                        var updateRes = await _userManager.UpdateAsync(identityUser);
                        if (!updateRes.Succeeded)
                        {
                            foreach (var e in updateRes.Errors)
                                ModelState.AddModelError(nameof(model.Email), e.Description);
                            PopulatePrograms();
                            return View(model);
                        }
                    }

                    // Promjena lozinke po checkboxu + sentinel
                    var wantsPasswordChange = model.ChangePassword
                                                   && !string.IsNullOrWhiteSpace(model.NewPassword)
                                                   && model.NewPassword != StudentskaSluzbaEditViewModel.PasswordSentinel;

                    if (wantsPasswordChange)
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(identityUser);
                        var passRes = await _userManager.ResetPasswordAsync(identityUser, token, model.NewPassword!);
                        if (!passRes.Succeeded)
                        {
                            foreach (var e in passRes.Errors)
                                ModelState.AddModelError(nameof(model.NewPassword), e.Description);
                            PopulatePrograms();
                            return View(model);
                        }
                    }
                }
            }

            // Veze sa studijskim programima: zamijeni set
            var stareVeze = ss.StudentskaSluzbaStudijskiProgrami.ToList();
            _context.StudentskaSluzbaStudijskiProgrami.RemoveRange(stareVeze);

            if (model.StudijskiProgramiIds != null && model.StudijskiProgramiIds.Any())
            {
                foreach (var programId in model.StudijskiProgramiIds.Distinct())
                {
                    _context.StudentskaSluzbaStudijskiProgrami.Add(new StudentskaSluzbaStudijskiProgram
                    {
                        StudentskaSluzbaId = ss.Id,
                        StudijskiProgramId = programId
                    });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: StudentskaSluzba/Delete/{id}
        [HttpGet("Delete/{id:long}")]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studentskaSluzba = await _context.StudentskeSluzbe
                .Include(ss => ss.StudentskaSluzbaStudijskiProgrami)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (studentskaSluzba == null)
            {
                return NotFound();
            }

            return View(studentskaSluzba);
        }

        // POST: StudentskaSluzba/Delete/{id}
        [HttpPost("Delete/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var studentskaSluzba = await _context.StudentskeSluzbe
                .Include(ss => ss.StudentskaSluzbaStudijskiProgrami)
                .FirstOrDefaultAsync(ss => ss.Id == id);

            if (studentskaSluzba != null)
            {
                // Prvo brišemo povezane studijske programe
                _context.StudentskaSluzbaStudijskiProgrami.RemoveRange(studentskaSluzba.StudentskaSluzbaStudijskiProgrami);

                // Brišemo studentsku službu
                _context.StudentskeSluzbe.Remove(studentskaSluzba);

                // Ako korisnik postoji u AspNetUsers tabeli, brišemo i njega
                var korisnik = await _context.Korisnici.FirstOrDefaultAsync(k => k.AspNetUserId == studentskaSluzba.Id.ToString());
                if (korisnik != null)
                {
                    _context.Korisnici.Remove(korisnik);
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool StudentskaSluzbaExists(long id)
        {
            return _context.StudentskeSluzbe.Any(e => e.Id == id);
        }
    }
}
