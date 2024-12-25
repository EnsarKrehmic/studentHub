using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;
using System.Diagnostics;

namespace StudentHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: localhost:port
        [HttpGet]
        [Route("")]
        [Route("[Controller]/[Action]")]
        public async Task<IActionResult> Index()
        {
            // Preuzimanje podataka iz baze
            var obavijesti = await _context.Obavjestenja
                .OrderByDescending(o => o.datumObjave)
                .Take(5)
                .ToListAsync();

            var viewModel = new HomeViewModel
            {
                NajnovijeObavijesti = obavijesti,
                BrojAsistenata = await _context.Asistenti.CountAsync(),
                BrojProfesora = await _context.Profesori.CountAsync(),
                BrojStudenata = await _context.Studenti.CountAsync()
            };

            return View(viewModel);
        }

        // GET: Home/Privacy
        [HttpGet]
        [Route("[Controller]/[Action]")]
        public IActionResult Privacy()
        {
            return View();
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

