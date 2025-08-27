using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;
using System.Security.Claims;
using X.PagedList.Extensions;

namespace StudentHub.Controllers
{
    [Route("Studenti")]
    public class StudentiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<PredmetiController> _logger;

        public StudentiController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<PredmetiController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Studenti
        [HttpGet("")]
        public async Task<IActionResult> Index(string sortOrder, string searchString, long? studijskiProgramId)
        {
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["SurnameSortParm"] = sortOrder == "surname_asc" ? "surname_desc" : "surname_asc";
            ViewData["JMBGSortParm"] = sortOrder == "jmbg_asc" ? "jmbg_desc" : "jmbg_asc";
            ViewData["IndexSortParm"] = sortOrder == "index_asc" ? "index_desc" : "index_asc";
            ViewData["EmailSortParm"] = sortOrder == "email_asc" ? "email_desc" : "email_asc";
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStudijskiProgramId"] = studijskiProgramId;

            var studentiQuery = _context.Studenti.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                studentiQuery = studentiQuery.Where(s => s.Ime.Contains(searchString) || s.Prezime.Contains(searchString));
            }

            if (studijskiProgramId.HasValue)
            {
                studentiQuery = studentiQuery.Where(p => _context.StudentStudijskiProgrami.Any(psp => psp.StudentId == p.Id && psp.StudijskiProgramId == studijskiProgramId.Value));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    studentiQuery = studentiQuery.OrderByDescending(s => s.Ime);
                    break;
                case "surname_asc":
                    studentiQuery = studentiQuery.OrderBy(s => s.Prezime);
                    break;
                case "surname_desc":
                    studentiQuery = studentiQuery.OrderByDescending(s => s.Prezime);
                    break;
                case "jmbg_asc":
                    studentiQuery = studentiQuery.OrderBy(s => s.JMBG);
                    break;
                case "jmbg_desc":
                    studentiQuery = studentiQuery.OrderByDescending(s => s.JMBG);
                    break;
                case "index_asc":
                    studentiQuery = studentiQuery.OrderBy(s => s.BrojIndeksa);
                    break;
                case "index_desc":
                    studentiQuery = studentiQuery.OrderByDescending(s => s.BrojIndeksa);
                    break;
                default:
                    studentiQuery = studentiQuery.OrderBy(s => s.Ime);
                    break;
            }

            var students = await studentiQuery.ToListAsync();

            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", studijskiProgramId);

            return View(students);
        }

        // GET: Studenti/Details{id}
        [HttpGet("Details/{id:long}")]
        [Authorize(Roles = "Student, Studentska služba, Profesor, Asistent")]
        public async Task<IActionResult> Details(long? id, string sortOrder, string searchString, long? studijskiProgramId)
        {
            if (id == null)
                return NotFound();

            var student = await _context.Studenti
                .Include(s => s.StudentStudijskiProgrami)
                    .ThenInclude(ssp => ssp.StudijskiProgram)
                .Include(s => s.StudentNaPredmetima)
                    .ThenInclude(snp => snp.Predmet)
                .FirstOrDefaultAsync(s => s.Id == id)
                ?? throw new InvalidOperationException("Student nije pronađen.");

            // Provjera vlasništva ako je Student
            if (User.IsInRole("Student"))
            {
                var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (student.AspNetUserId != loggedInUserId)
                    return Forbid();
            }

            var studijskiProgram = student.StudentStudijskiProgrami.FirstOrDefault()?.StudijskiProgram;
            var studijskiProgramIdForLimit = studijskiProgram?.Id ?? 0;

            // Predmeti koje pohađa
            var predmeti = await _context.StudentiNaPredmetima
                .Include(snp => snp.Predmet)
                .Where(snp => snp.StudentId == id)
                .Select(snp => snp.Predmet)
                .ToListAsync();

            // Sve ocjene (predmetne)
            var sveOcjene = await _context.Ocjene
                .Include(o => o.Predmet)
                .Include(o => o.Profesor)
                .Where(o => o.StudentId == student.Id && o.Tip == TipOcjene.Predmet)
                .ToListAsync();

            var glavne = sveOcjene.Where(o => o.ParentOcjenaId == null).ToList();
            var parcijalne = sveOcjene.Where(o => o.ParentOcjenaId != null).ToList();

            var grupisaneParcijalne = parcijalne
                .GroupBy(o => o.ParentOcjenaId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            var ocjeneViewModels = glavne.Select(o =>
            {
                float? ponderisana = null;

                if (grupisaneParcijalne.TryGetValue(o.Id, out var parcijalneZaOvu) && parcijalneZaOvu.Any(p => p.TezinaProcentualno.HasValue))
                {
                    var suma = parcijalneZaOvu.Sum(p => p.Vrijednost * ((p.TezinaProcentualno ?? 0) / 100f));
                    ponderisana = suma;
                }

                return new OcjenaViewModel
                {
                    Id = o.Id,
                    Tip = o.Tip.ToString(),
                    PredmetId = o.PredmetId ?? 0,
                    PredmetNaziv = o.Predmet?.Naziv ?? "",
                    StudentIme = student.Ime,
                    StudentPrezime = student.Prezime,
                    StudentBrojIndeksa = student.BrojIndeksa,
                    StudentStudijskiProgramNaziv = studijskiProgram?.Naziv ?? "",
                    ProfesorIme = o.Profesor?.Ime ?? "",
                    ProfesorPrezime = o.Profesor?.Prezime ?? "",
                    ProfesorTitula = o.Profesor?.ProfesorTitula ?? "",
                    Vrijednost = o.Vrijednost,
                    TezinaProcentualno = o.TezinaProcentualno,
                    Komentar = o.Komentar,
                    DatumDodjele = o.DatumUnosa,
                    DjelimicneOcjene = grupisaneParcijalne.ContainsKey(o.Id)
                        ? grupisaneParcijalne[o.Id].Select(p => new OcjenaViewModel
                        {
                            Vrijednost = p.Vrijednost,
                            TezinaProcentualno = p.TezinaProcentualno,
                            Komentar = p.Komentar,
                            DatumDodjele = p.DatumUnosa
                        }).ToList()
                        : new List<OcjenaViewModel>(),
                    StudentId = student.Id,
                    ProfesorId = o.ProfesorId,
                    StudijskiProgramId = studijskiProgramIdForLimit,
                    ProsjekPrikaz = ponderisana.HasValue ? ponderisana.Value.ToString("0.00") : null
                };
            }).ToList();

            // Dohvati LIMIT ako postoji
            StudijskiProgramIzborniLimit? limit = null;
            if (student.GodinaStudija.HasValue && studijskiProgramIdForLimit > 0)
            {
                limit = await _context.StudijskiProgramIzborniLimiti
                    .FirstOrDefaultAsync(l =>
                        l.StudijskiProgramId == studijskiProgramIdForLimit &&
                        l.GodinaStudija == student.GodinaStudija.Value);
            }

            // Kreiraj view model
            var viewModel = new StudentDetailsViewModel
            {
                Student = student,
                CurrentSort = sortOrder,
                SearchString = searchString,
                StudijskiProgramId = studijskiProgramId,
                Predmeti = predmeti,
                OcjenePredmeta = ocjeneViewModels,
                StudijskiProgramIzborniLimit = limit
            };

            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", studijskiProgramId);
            return View(viewModel);
        }

        // GET: Studenti/Create
        [HttpGet("Create")]
        [Authorize(Roles = "Studentska služba")]
        public IActionResult Create()
        {
            ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv");
            ViewBag.NastavniPlanovi = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
            ViewBag.Predmeti = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
            return View();
        }


        // POST: Studenti/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Create(StudentCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // repopulate dropdowns (koristi Program + Godina + Semestar ako su uneseni)
                ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", model.StudijskiProgramId);

                // filtriraj planove po programu
                ViewBag.NastavniPlanovi = model.StudijskiProgramId != 0
                    ? new SelectList(
                        await _context.NastavniPlanovi
                            .Where(np => np.StudijskiProgramId == model.StudijskiProgramId)
                            .Select(np => new
                            {
                                Id = np.Id,
                                Naziv = $"Nastavni plan za {np.StudijskiProgram.Naziv}: {np.GodinaStudija}. godinu"
                            }).ToListAsync(),
                        "Id", "Naziv", model.NastavniPlanId)
                    : new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");

                // ako znamo i godinu → pronađi konkretan plan i filtriraj predmete po njemu (+ semestar ako je postavljen)
                if (model.StudijskiProgramId != 0 && model.GodinaStudija.HasValue)
                {
                    var np = await _context.NastavniPlanovi.FirstOrDefaultAsync(x =>
                        x.StudijskiProgramId == model.StudijskiProgramId &&
                        x.GodinaStudija == model.GodinaStudija.Value.ToString());

                    if (np != null)
                    {
                        var predmeti = await _context.Predmeti
                            .Where(p => p.NastavniPlanId == np.Id)
                            .Where(p => model.Semestar == null || p.Semestar == model.Semestar)
                            .Select(p => new { p.Id, p.Naziv })
                            .ToListAsync();

                        ViewBag.Predmeti = new SelectList(predmeti, "Id", "Naziv", model.PredmetIds);
                    }
                    else
                    {
                        ViewBag.Predmeti = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
                    }
                }
                else
                {
                    ViewBag.Predmeti = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
                }

                return View(model);
            }

            // 0) Pronađi Nastavni plan po (StudijskiProgramId + GodinaStudija)
            var nastavniPlan = await _context.NastavniPlanovi
                .FirstOrDefaultAsync(np =>
                    np.StudijskiProgramId == model.StudijskiProgramId &&
                    np.GodinaStudija == model.GodinaStudija!.Value.ToString());

            if (nastavniPlan == null)
            {
                ModelState.AddModelError(nameof(model.NastavniPlanId),
                    "Nastavni plan za odabrani program i godinu studija nije pronađen.");

                ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", model.StudijskiProgramId);
                ViewBag.NastavniPlanovi = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
                ViewBag.Predmeti = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
                return View(model);
            }

            // 1) Kreiranje Identity User-a
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

                ViewBag.StudijskiProgrami = new SelectList(_context.StudijskiProgrami, "Id", "Naziv", model.StudijskiProgramId);
                ViewBag.NastavniPlanovi = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
                ViewBag.Predmeti = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
                return View(model);
            }

            // 2) Rola
            try { await _userManager.AddToRoleAsync(identityUser, "Student"); } catch { }

            // 3) Student entitet
            var student = new Student
            {
                AspNetUserId = identityUser.Id,
                JMBG = model.JMBG,
                Ime = model.Ime,
                Prezime = model.Prezime,
                Email = model.Email,
                BrojIndeksa = model.BrojIndeksa,
                PrethodnoObrazovanje = model.PrethodnoObrazovanje,
                GodinaStudija = model.GodinaStudija,
                Semestar = model.Semestar,
                Uloga = Uloga.Student,
                NastavniPlanId = nastavniPlan.Id
            };

            _context.Studenti.Add(student);
            await _context.SaveChangesAsync();

            // 4) StudentStudijskiProgram
            if (model.StudijskiProgramId != 0)
            {
                _context.StudentStudijskiProgrami.Add(new StudentStudijskiProgram
                {
                    StudentId = student.Id,
                    StudijskiProgramId = model.StudijskiProgramId
                });
            }

            // 5) Predmeti: program + godina → plan; semestar (1/2) opcionalno
            var predmetiUPrihvacenomPlanu = await _context.Predmeti
                .Where(p => p.NastavniPlanId == nastavniPlan.Id)
                .Where(p => model.Semestar == null || p.Semestar == model.Semestar)
                .Select(p => new { p.Id })
                .ToListAsync();

            IEnumerable<long> predmetIdsZaUpis;

            if (model.PredmetIds != null && model.PredmetIds.Any())
            {
                var validIds = new HashSet<long>(predmetiUPrihvacenomPlanu.Select(x => x.Id));
                predmetIdsZaUpis = model.PredmetIds.Where(id => validIds.Contains(id));
            }
            else
            {
                predmetIdsZaUpis = predmetiUPrihvacenomPlanu.Select(x => x.Id);
            }

            if (predmetIdsZaUpis.Any())
            {
                foreach (var predmetId in predmetIdsZaUpis)
                {
                    _context.StudentiNaPredmetima.Add(new StudentNaPredmetu
                    {
                        StudentId = student.Id,
                        PredmetId = predmetId,
                        AkademskaGodina = DateTime.Now.Year.ToString(),
                        AspNetUserId = identityUser.Id
                    });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Studenti/Edit/{id}
        [HttpGet("Edit/{id:long}")]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Edit(long id)
        {
            var student = await _context.Studenti
                .Include(s => s.StudentStudijskiProgrami)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
                return NotFound();

            var studijskiProgramId = student.StudentStudijskiProgrami.FirstOrDefault()?.StudijskiProgramId ?? 0;

            // Identity e-mail kao izvor istine
            IdentityUser? identityUser = null;
            if (!string.IsNullOrWhiteSpace(student.AspNetUserId))
                identityUser = await _userManager.FindByIdAsync(student.AspNetUserId);

            // Već upisani predmetId-ovi
            var upisaniPredmetIds = await _context.StudentiNaPredmetima
                .Where(snp => snp.StudentId == student.Id)
                .Select(snp => snp.PredmetId)
                .ToListAsync();

            var model = new StudentEditViewModel
            {
                Id = student.Id,
                Ime = student.Ime,
                Prezime = student.Prezime,
                JMBG = student.JMBG,
                Email = identityUser?.Email ?? student.Email,
                BrojIndeksa = student.BrojIndeksa,
                StudijskiProgramId = studijskiProgramId,
                GodinaStudija = student.GodinaStudija,
                Semestar = student.Semestar,
                PrethodnoObrazovanje = student.PrethodnoObrazovanje,
                Uloga = student.Uloga,
                IzborIzbornihPredmetaZakljucan = student.IzborIzbornihPredmetaZakljucan,
                PredmetIds = upisaniPredmetIds,

                // Sentinel popuna
                NewPassword = StudentEditViewModel.PasswordSentinel,
                ConfirmNewPassword = StudentEditViewModel.PasswordSentinel,
                ChangePassword = false
            };

            // Dropdown-i (programi i planovi po programu)
            ViewBag.StudijskiProgrami = new SelectList(
                await _context.StudijskiProgrami.ToListAsync(), "Id", "Naziv", studijskiProgramId);

            ViewBag.NastavniPlanovi = new SelectList(
                await _context.NastavniPlanovi
                    .Where(np => np.StudijskiProgramId == studijskiProgramId)
                    .Select(np => new
                    {
                        Id = np.Id,
                        Naziv = $"Nastavni plan za {np.StudijskiProgram.Naziv}: {np.GodinaStudija}. godinu"
                    })
                    .ToListAsync(),
                "Id", "Naziv", student.NastavniPlanId);

            // PREDMETI: unija (predmeti iz AKTUELNOG plana (+ semestar) ∪ već upisani)
            var planPredmeti = await _context.Predmeti
                .Where(p => p.NastavniPlanId == student.NastavniPlanId)
                .Where(p => student.Semestar == null || p.Semestar == student.Semestar) // ako postoji kolona p.Semestar
                .Select(p => new { p.Id, p.Naziv })
                .ToListAsync();

            var upisaniPredmeti = await _context.Predmeti
                .Where(p => upisaniPredmetIds.Contains(p.Id))
                .Select(p => new { p.Id, p.Naziv })
                .ToListAsync();

            var predmetiUnion = planPredmeti
                .Concat(upisaniPredmeti)
                .GroupBy(x => x.Id)
                .Select(g => g.First())
                .OrderBy(x => x.Naziv)
                .ToList();

            ViewBag.Predmeti = new SelectList(predmetiUnion, "Id", "Naziv", model.PredmetIds);

            return View(model);
        }

        // POST: Studenti/Edit/{id}
        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Edit(long id, StudentEditViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            // Helper za repop ViewBag.Predmeti sa UNIJOM (plan + selektovani) kada treba vratiti View
            async Task PopulateDropdownsAsync()
            {
                ViewBag.StudijskiProgrami = new SelectList(
                    await _context.StudijskiProgrami.ToListAsync(), "Id", "Naziv", model.StudijskiProgramId);

                ViewBag.NastavniPlanovi = new SelectList(
                    await _context.NastavniPlanovi
                        .Where(np => np.StudijskiProgramId == model.StudijskiProgramId)
                        .Select(np => new
                        {
                            Id = np.Id,
                            Naziv = $"Nastavni plan za {np.StudijskiProgram.Naziv}: {np.GodinaStudija}. godinu"
                        })
                        .ToListAsync(),
                    "Id", "Naziv", model.NastavniPlanId);

                // plan predmeti (ako možemo odrediti plan po programu + godini)
                List<dynamic> planPredmeti;
                if (model.GodinaStudija.HasValue)
                {
                    var planId = await _context.NastavniPlanovi
                        .Where(np => np.StudijskiProgramId == model.StudijskiProgramId
                                  && np.GodinaStudija == model.GodinaStudija.Value.ToString())
                        .Select(np => np.Id)
                        .FirstOrDefaultAsync();

                    planPredmeti = planId == 0
                        ? new List<dynamic>()
                        : await _context.Predmeti
                            .Where(p => p.NastavniPlanId == planId)
                            .Where(p => model.Semestar == null || p.Semestar == model.Semestar) // ako postoji p.Semestar
                            .Select(p => new { Id = p.Id, Naziv = p.Naziv })
                            .ToListAsync<dynamic>();
                }
                else
                {
                    // fallback po programu
                    planPredmeti = await _context.Predmeti
                        .Where(p => p.NastavniPlan.StudijskiProgramId == model.StudijskiProgramId)
                        .Select(p => new { Id = p.Id, Naziv = p.Naziv })
                        .ToListAsync<dynamic>();
                }

                // već selektovani (da ostanu vidljivi)
                var selektovaniIds = model.PredmetIds ?? new List<long>();
                var selektovaniPredmeti = await _context.Predmeti
                    .Where(p => selektovaniIds.Contains(p.Id))
                    .Select(p => new { Id = p.Id, Naziv = p.Naziv })
                    .ToListAsync();

                // Unija po Id
                var predmetiUnion = planPredmeti
                    .Concat(selektovaniPredmeti)
                    .GroupBy(x => (long)x.Id)
                    .Select(g => new { Id = g.Key, Naziv = g.First().Naziv })
                    .OrderBy(x => x.Naziv)
                    .ToList();

                ViewBag.Predmeti = new SelectList(predmetiUnion, "Id", "Naziv", model.PredmetIds);
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(model);
            }

            var existingStudent = await _context.Studenti
                .Include(s => s.StudentStudijskiProgrami)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (existingStudent == null)
                return NotFound();

            // Guard prije čitanja plana
            if (!model.GodinaStudija.HasValue)
            {
                ModelState.AddModelError(nameof(model.GodinaStudija), "Godina studija je obavezna.");
                await PopulateDropdownsAsync();
                return View(model);
            }

            // Ažuriranje poslovnih podataka
            existingStudent.Ime = model.Ime;
            existingStudent.Prezime = model.Prezime;
            existingStudent.JMBG = model.JMBG;
            existingStudent.Email = model.Email;
            existingStudent.BrojIndeksa = model.BrojIndeksa;
            existingStudent.PrethodnoObrazovanje = model.PrethodnoObrazovanje;
            existingStudent.GodinaStudija = model.GodinaStudija;
            existingStudent.Semestar = model.Semestar;
            existingStudent.Uloga = model.Uloga;
            existingStudent.IzborIzbornihPredmetaZakljucan = model.IzborIzbornihPredmetaZakljucan;

            // Nastavni plan -> (program + GodinaStudija)
            var nastavniPlan = await _context.NastavniPlanovi
                .FirstOrDefaultAsync(np => np.StudijskiProgramId == model.StudijskiProgramId
                                        && np.GodinaStudija == model.GodinaStudija.Value.ToString());

            if (nastavniPlan == null)
            {
                ModelState.AddModelError(nameof(model.NastavniPlanId), "Nastavni plan za odabranu godinu studija nije pronađen.");
                await PopulateDropdownsAsync();
                return View(model);
            }

            existingStudent.NastavniPlanId = nastavniPlan.Id;

            // Identity e-mail i lozinka
            if (!string.IsNullOrWhiteSpace(existingStudent.AspNetUserId))
            {
                var identityUser = await _userManager.FindByIdAsync(existingStudent.AspNetUserId);
                if (identityUser != null)
                {
                    // Update e-mail/username ako je promijenjen
                    if (!string.Equals(identityUser.Email, model.Email, StringComparison.OrdinalIgnoreCase))
                    {
                        identityUser.Email = model.Email;
                        identityUser.UserName = model.Email; // ako koristite e-mail kao username
                        var updateRes = await _userManager.UpdateAsync(identityUser);
                        if (!updateRes.Succeeded)
                        {
                            foreach (var e in updateRes.Errors)
                                ModelState.AddModelError(nameof(model.Email), e.Description);

                            await PopulateDropdownsAsync();
                            return View(model);
                        }
                    }

                    // Reset lozinke samo ako je eksplicitno traženo i vrijednost != sentinel
                    var wantsPasswordChange = model.ChangePassword
                                                   && !string.IsNullOrWhiteSpace(model.NewPassword)
                                                   && model.NewPassword != StudentEditViewModel.PasswordSentinel;

                    if (wantsPasswordChange)
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(identityUser);
                        var passRes = await _userManager.ResetPasswordAsync(identityUser, token, model.NewPassword!);
                        if (!passRes.Succeeded)
                        {
                            foreach (var e in passRes.Errors)
                                ModelState.AddModelError(nameof(model.NewPassword), e.Description);

                            await PopulateDropdownsAsync();
                            return View(model);
                        }
                    }
                }
            }

            // CARRY-OVER: dozvoli predmete iz BILO KOJE godine istog studijskog programa
            var validPredmetiIds = await _context.Predmeti
                .Where(p => p.NastavniPlan.StudijskiProgramId == model.StudijskiProgramId)
                .Select(p => p.Id)
                .ToListAsync();

            var validSet = new HashSet<long>(validPredmetiIds);
            var predmetIdsZaUpis = (model.PredmetIds ?? new List<long>())
                .Where(id => validSet.Contains(id))
                .Distinct()
                .ToList();

            // Zamijeni set predmeta
            var stariPredmeti = await _context.StudentiNaPredmetima
                .Where(snp => snp.StudentId == existingStudent.Id)
                .ToListAsync();

            _context.StudentiNaPredmetima.RemoveRange(stariPredmeti);

            if (predmetIdsZaUpis.Any())
            {
                foreach (var predmetId in predmetIdsZaUpis)
                {
                    _context.StudentiNaPredmetima.Add(new StudentNaPredmetu
                    {
                        StudentId = existingStudent.Id,
                        PredmetId = predmetId,
                        AkademskaGodina = DateTime.Now.Year.ToString(),
                        AspNetUserId = existingStudent.AspNetUserId
                    });
                }
            }

            // StudentStudijskiProgram: zamijeni
            var existingEntries = _context.StudentStudijskiProgrami
                .Where(ssp => ssp.StudentId == existingStudent.Id)
                .ToList();

            _context.StudentStudijskiProgrami.RemoveRange(existingEntries);

            if (model.StudijskiProgramId != 0)
            {
                _context.StudentStudijskiProgrami.Add(new StudentStudijskiProgram
                {
                    StudentId = existingStudent.Id,
                    StudijskiProgramId = model.StudijskiProgramId
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Studenti/Delete/{id}
        [HttpGet("Delete/{id:long}")]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Studenti.FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Studenti/Delete/{id}
        [HttpPost("Delete/{id:long}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Studentska služba")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var student = await _context.Studenti.FindAsync(id);
            if (student != null)
            {
                _context.Studenti.Remove(student);
                var korisnik = await _context.Korisnici.FindAsync(id);
                if (korisnik != null)
                {
                    _context.Korisnici.Remove(korisnik);
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(long id)
        {
            return _context.Studenti.Any(e => e.Id == id);
        }

        [HttpGet("GroupedByProgram")]
        public async Task<IActionResult> GroupedByProgram(string sortOrder, string searchString)
        {
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["SurnameSortParm"] = sortOrder == "surname_asc" ? "surname_desc" : "surname_asc";
            ViewData["IndexSortParm"] = sortOrder == "index_asc" ? "index_desc" : "index_asc";
            ViewData["CurrentFilter"] = searchString;

            var studentiQuery = _context.Studenti
                .Include(s => s.StudentStudijskiProgrami)
                .ThenInclude(ssp => ssp.StudijskiProgram)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                studentiQuery = studentiQuery.Where(s => s.Ime.Contains(searchString) || s.Prezime.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    studentiQuery = studentiQuery.OrderByDescending(s => s.Ime);
                    break;
                case "surname_asc":
                    studentiQuery = studentiQuery.OrderBy(s => s.Prezime);
                    break;
                case "surname_desc":
                    studentiQuery = studentiQuery.OrderByDescending(s => s.Prezime);
                    break;
                case "index_asc":
                    studentiQuery = studentiQuery.OrderBy(s => s.BrojIndeksa);
                    break;
                case "index_desc":
                    studentiQuery = studentiQuery.OrderByDescending(s => s.BrojIndeksa);
                    break;
                default:
                    studentiQuery = studentiQuery.OrderBy(s => s.Ime);
                    break;
            }

            var groupedStudents = await studentiQuery
                .GroupBy(s => s.StudentStudijskiProgrami.FirstOrDefault().StudijskiProgram)
                .Select(g => new StudentGroupedByProgramViewModel
                {
                    StudijskiProgram = g.Key,
                    Studenti = g.ToList()
                })
                .ToListAsync();

            return View(groupedStudents);
        }

        // GET: /Studenti/GetPredmetiByStudijskiProgram/{studijskiProgramId}?godinaStudija=1..6&semestar=1..2
        [HttpGet("GetPredmetiByStudijskiProgram/{studijskiProgramId}")]
        public async Task<IActionResult> GetPredmetiByStudijskiProgram(long studijskiProgramId, [FromQuery] int? godinaStudija, [FromQuery] int? semestar)
        {
            if (studijskiProgramId == 0)
                return Json(Array.Empty<object>());

            IQueryable<Predmet> q = _context.Predmeti.AsQueryable();

            if (godinaStudija.HasValue)
            {
                // Nađi tačan nastavni plan za (program + godina)
                var planId = await _context.NastavniPlanovi
                    .Where(np => np.StudijskiProgramId == studijskiProgramId && np.GodinaStudija == godinaStudija.Value.ToString())
                    .Select(np => np.Id)
                    .FirstOrDefaultAsync();

                if (planId == 0)
                    return Json(Array.Empty<object>());

                q = q.Where(p => p.NastavniPlanId == planId);
            }
            else
            {
                // Bez godine: vrati sve predmete svih planova tog programa (kao ranije ponašanje)
                q = q.Where(p => p.NastavniPlan.StudijskiProgramId == studijskiProgramId);
            }

            if (semestar.HasValue)
                q = q.Where(p => p.Semestar == semestar.Value);

            var predmeti = await q
                .OrderBy(p => p.Naziv)
                .Select(p => new { id = p.Id, naziv = p.Naziv })
                .ToListAsync();

            return Json(predmeti);
        }

        // GET: /Studenti/GetNastavniPlanovi/{studijskiProgramId}
        [HttpGet("GetNastavniPlanovi/{studijskiProgramId}")]
        public async Task<IActionResult> GetNastavniPlanovi(long studijskiProgramId)
        {
            var nastavaniPlanovi = await _context.NastavniPlanovi
                .Where(np => np.StudijskiProgramId == studijskiProgramId)
                .Select(np => new
                {
                    id = np.Id,
                    // Vraćamo i numeric 'godinaStudija' kako JS ne bi parsirao string
                    godinaStudija = np.GodinaStudija,
                    naziv = $"Nastavni plan za {np.StudijskiProgram.Naziv}: {np.GodinaStudija}. godinu"
                })
                .ToListAsync();

            return Json(nastavaniPlanovi);
        }

        [HttpGet("BirajIzbornePredmete")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> BirajIzbornePredmete()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var student = await _context.Studenti
                .Include(s => s.StudentStudijskiProgrami).ThenInclude(ssp => ssp.StudijskiProgram)
                .FirstOrDefaultAsync(s => s.AspNetUserId == userId);

            if (student == null)
            {
                return NotFound();
            }

            if (student.GodinaStudija == null || student.GodinaStudija <= 0)
            {
                TempData["ErrorMessage"] = "Vašoj studentskoj evidenciji nedostaje godina studija. Molimo kontaktirajte Studentsku službu.";
                return RedirectToAction("Details", "Studenti", new { id = student.Id });
            }

            var godinaStudija = student.GodinaStudija.Value;
            var studijskiProgramId = student.StudentStudijskiProgrami.FirstOrDefault()?.StudijskiProgramId ?? 0;
            var studijskiProgram = await _context.StudijskiProgrami.FirstOrDefaultAsync(sp => sp.Id == studijskiProgramId);

            var limit = await _context.StudijskiProgramIzborniLimiti
                .FirstOrDefaultAsync(l => l.StudijskiProgramId == studijskiProgramId && l.GodinaStudija == godinaStudija);

            if (limit == null)
            {
                TempData["ErrorMessage"] = "Za Vaš studijski program i godinu nije postavljen limit za izborne predmete. Molimo kontaktirajte Studentsku službu.";
                return RedirectToAction("Details", "Studenti", new { id = student.Id });
            }

            var izborniPredmeti = await _context.Predmeti
                .Where(p => p.TipPredmeta == TipPredmeta.Izborni &&
                            p.NastavniPlan.StudijskiProgramId == studijskiProgramId &&
                            p.NastavniPlan.GodinaStudija == godinaStudija.ToString())
                .ToListAsync();

            if (!izborniPredmeti.Any())
            {
                TempData["ErrorMessage"] = "Trenutno nema dostupnih izbornih predmeta za Vaš studijski program i godinu.";
                return RedirectToAction("Details", "Studenti", new { id = student.Id });
            }

            var odabraniPredmetIds = await _context.StudentiNaPredmetima
                .Where(snp => snp.StudentId == student.Id && snp.Predmet.TipPredmeta == TipPredmeta.Izborni)
                .Select(snp => snp.PredmetId)
                .ToListAsync();

            var brojVecOdabranih = odabraniPredmetIds.Count;

            var model = new BirajIzbornePredmeteViewModel
            {
                StudentId = student.Id,
                ImePrezime = $"{student.Ime} {student.Prezime}",
                GodinaStudija = godinaStudija,
                StudijskiProgramId = studijskiProgramId,
                StudijskiProgramNaziv = studijskiProgram?.Naziv ?? "",
                MinIzborniPredmeti = limit.MinIzborniPredmeti,
                MaxIzborniPredmeti = limit.MaxIzborniPredmeti,
                BrojVecOdabranihPredmeta = brojVecOdabranih,
                IsLocked = student.IzborIzbornihPredmetaZakljucan || brojVecOdabranih >= limit.MaxIzborniPredmeti,
                Predmeti = izborniPredmeti.Select(p => new PredmetCheckboxViewModel
                {
                    PredmetId = p.Id,
                    Naziv = p.Naziv,
                    IsSelected = odabraniPredmetIds.Contains(p.Id)
                }).ToList()
            };

            return View(model);
        }

        [HttpPost("BirajIzbornePredmete")]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BirajIzbornePredmete(BirajIzbornePredmeteViewModel model)
        {
            _logger.LogInformation("POST BirajIzbornePredmete pozvan.");
            _logger.LogInformation("SelectedPredmetiIds count: {Count}", model.SelectedPredmetiIds?.Count ?? 0);

            if (model.SelectedPredmetiIds.Count < model.MinIzborniPredmeti)
            {
                TempData["ErrorMessage"] = $"Morate odabrati najmanje {model.MinIzborniPredmeti} izbornih predmeta.";
                return RedirectToAction("BirajIzbornePredmete");
            }

            if (model.SelectedPredmetiIds.Count > model.MaxIzborniPredmeti)
            {
                TempData["ErrorMessage"] = $"Možete odabrati najviše {model.MaxIzborniPredmeti} izbornih predmeta.";
                return RedirectToAction("BirajIzbornePredmete");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _context.Studenti.FirstOrDefaultAsync(s => s.AspNetUserId == userId);
            if (student == null)
            {
                return NotFound();
            }

            if (student.IzborIzbornihPredmetaZakljucan)
            {
                TempData["ErrorMessage"] = "Vaš izbor izbornih predmeta je zaključan i ne može se mijenjati.";
                return RedirectToAction("BirajIzbornePredmete");
            }

            var stariIzborniPredmeti = await _context.StudentiNaPredmetima
                .Where(snp => snp.StudentId == student.Id && snp.Predmet.TipPredmeta == TipPredmeta.Izborni)
                .ToListAsync();

            _context.StudentiNaPredmetima.RemoveRange(stariIzborniPredmeti);

            if (model.SelectedPredmetiIds != null && model.SelectedPredmetiIds.Any())
            {
                foreach (var predmetId in model.SelectedPredmetiIds)
                {
                    _context.StudentiNaPredmetima.Add(new StudentNaPredmetu
                    {
                        StudentId = student.Id,
                        PredmetId = predmetId,
                        AkademskaGodina = DateTime.Now.Year.ToString(),
                        AspNetUserId = student.AspNetUserId
                    });
                }
            }

            // Zaključavamo SAMO AKO je odabrao tačno Max broj
            if (model.SelectedPredmetiIds.Count == model.MaxIzborniPredmeti)
            {
                student.IzborIzbornihPredmetaZakljucan = true;
                TempData["SuccessMessage"] = "Odabrali ste maksimalan broj izbornih predmeta. Vaš izbor je sada zaključan.";
            }
            else
            {
                TempData["SuccessMessage"] = "Vaš izbor je sačuvan. Možete naknadno dodati još izbornih predmeta do maksimalnog limita.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("BirajIzbornePredmete");
        }

        [HttpPost("ZakljucajIzborIzbornihPredmeta")]
        [Authorize(Roles = "Studentska služba")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ZakljucajIzborIzbornihPredmeta(long id)
        {
            var student = await _context.Studenti.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            student.IzborIzbornihPredmetaZakljucan = true;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Izbor izbornih predmeta je uspješno zaključan.";
            return RedirectToAction("Details", new { id });
        }

        [HttpPost("OtkjucajIzborIzbornihPredmeta")]
        [Authorize(Roles = "Studentska služba")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OtkjucajIzborIzbornihPredmeta(long id)
        {
            var student = await _context.Studenti.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            student.IzborIzbornihPredmetaZakljucan = false;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Izbor izbornih predmeta je uspješno otključan.";
            return RedirectToAction("Details", new { id });
        }
    }
}