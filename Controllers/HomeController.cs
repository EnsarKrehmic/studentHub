using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;
using System.Diagnostics;
using System.Security.Claims;

namespace StudentHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        // GET: localhost:port
        public async Task<IActionResult> Index()
        {
            // Učitavanje najnovijih obavijesti
            var obavijesti = await _context.Obavjestenja
                .Include(o => o.ObavjestenjeStudijskiProgrami).ThenInclude(os => os.StudijskiProgram)
                .Include(o => o.Korisnik)
                .Include(o => o.StudentskaSluzba)
                .Include(o => o.Profesor)
                .Include(o => o.Asistent)
                .OrderByDescending(o => o.DatumObjave)
                .Take(5)
                .Select(o => new ObavjestenjeViewModel
                {
                    Id = o.Id,
                    Naslov = o.Naslov,
                    Sadrzaj = o.Sadrzaj,
                    DatumObjave = o.DatumObjave,
                    StudijskiProgramNazivi = o.ObavjestenjeStudijskiProgrami
                        .Select(os => os.StudijskiProgram.Naziv)
                        .ToList(),
                    AutorIme = o.Korisnik != null ? $"{o.Korisnik.Ime} {o.Korisnik.Prezime}" :
                                o.StudentskaSluzba != null ? $"{o.StudentskaSluzba.Ime} {o.StudentskaSluzba.Prezime}" :
                                o.Profesor != null ? $"{o.Profesor.Ime} {o.Profesor.Prezime}" :
                                o.Asistent != null ? $"{o.Asistent.Ime} {o.Asistent.Prezime}" : "Nepoznato"
                })
                .ToListAsync();

            // Učitavanje broja asistenata, profesora, studenata, i ispita
            var homeViewModel = new HomeViewModel
            {
                NajnovijeObavijesti = obavijesti,
                BrojAsistenata = await _context.Asistenti.CountAsync(),
                BrojProfesora = await _context.Profesori.CountAsync(),
                BrojStudenata = await _context.Studenti.CountAsync(),
                AktivniIspiti = await _context.Ispiti.CountAsync(),

                // Učitavanje svih studijskih programa za prikaz na početnoj stranici
                StudijskiProgrami = await _context.StudijskiProgrami
                    .OrderBy(sp => sp.Naziv)
                    .ToListAsync()
            };

            // Sinhronizacija korisnika
            var claimsIdentity = User.Identity as ClaimsIdentity;
            if (claimsIdentity != null)
            {
                var userIdClaim = claimsIdentity.Claims
                    .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

                if (userIdClaim != null)
                {
                    var userId = userIdClaim.Value;
                    var existingUser = await _context.Korisnici.FirstOrDefaultAsync(k => k.AspNetUserId == userId);

                    if (existingUser == null)
                    {
                        // Dohvati usera iz Identity sistema
                        var user = await _userManager.FindByIdAsync(userId);
                        if (user != null)
                        {
                            // Dohvati uloge korisnika
                            var roles = await _userManager.GetRolesAsync(user);
                            var userRoleString = roles.FirstOrDefault() ?? "Osnovni";

                            // Pravilno mapiranje stringa na enum
                            if (!Enum.TryParse(userRoleString.Replace(" ", ""), true, out Uloga userRoleEnum))
                            {
                                userRoleEnum = Uloga.Osnovni; // Fallback na osnovnog korisnika ako nešto ne štima
                            }

                            // Lozinka (hashirana iz Identity sistema)
                            var hashedPassword = user.PasswordHash;

                            _context.Korisnici.Add(new Korisnik()
                            {
                                AspNetUserId = userId,
                                JMBG = "",
                                Ime = "",
                                Prezime = "",
                                Email = user.Email,
                                Lozinka = hashedPassword,
                                Uloga = userRoleEnum
                            });

                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }

            // Vraćamo view nakon što je sve obavljeno
            return View(homeViewModel);
        }

        // GET: Home/Error
        [HttpGet]
        [Route("[Controller]/[Action]")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
