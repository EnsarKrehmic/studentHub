using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;

namespace StudentHub.Controllers
{
    [Route("StatistikaIzbornihPredmeta")]
    public class StatistikaIzbornihPredmetaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StatistikaIzbornihPredmetaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: StatistikaIzbornihPredmeta
        [HttpGet("")]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            // Dohvati sve studijske programe
            var sviProgrami = await _context.StudijskiProgrami
                .OrderBy(sp => sp.Naziv)
                .ToListAsync();

            var result = new List<IzborniPredmetStatistikaGroupedViewModel>();

            foreach (var program in sviProgrami)
            {
                var groupedViewModel = new IzborniPredmetStatistikaGroupedViewModel
                {
                    StudijskiProgramNaziv = program.Naziv,
                    GodineStudija = new List<IzborniPredmetStatistikaGodinaViewModel>()
                };

                // Dohvati sve godine koje imaju definisan NastavniPlan za ovaj program
                var godine = await _context.NastavniPlanovi
                    .Where(np => np.StudijskiProgramId == program.Id)
                    .Select(np => np.GodinaStudija)
                    .Distinct()
                    .OrderBy(g => g)
                    .ToListAsync();

                foreach (var godina in godine)
                {
                    // Dohvati sve izborne predmete za tu godinu i program
                    var izborniPredmeti = await _context.Predmeti
                        .Where(p => p.TipPredmeta == TipPredmeta.Izborni &&
                                    p.NastavniPlan.StudijskiProgramId == program.Id &&
                                    p.NastavniPlan.GodinaStudija == godina)
                        .ToListAsync();

                    // Ukupan broj studenata u toj godini i programu
                    var studentiCount = await _context.Studenti
                        .Where(s =>
                            s.StudentStudijskiProgrami.Any(ssp => ssp.StudijskiProgramId == program.Id) &&
                            s.GodinaStudija.ToString() == godina)
                        .CountAsync();

                    if (studentiCount == 0)
                    {
                        studentiCount = 1; // da izbjegnemo dijeljenje sa 0
                    }

                    var statistikaGodina = new IzborniPredmetStatistikaGodinaViewModel
                    {
                        GodinaStudija = int.Parse(godina),
                        Statistika = new List<IzborniPredmetStatistikaViewModel>()
                    };

                    foreach (var predmet in izborniPredmeti)
                    {
                        var brojOdabira = await _context.StudentiNaPredmetima
                            .Where(snp => snp.PredmetId == predmet.Id)
                            .CountAsync();

                        statistikaGodina.Statistika.Add(new IzborniPredmetStatistikaViewModel
                        {
                            NazivPredmeta = predmet.Naziv,
                            BrojStudenata = brojOdabira,
                            Procenat = (double)brojOdabira / studentiCount * 100
                        });
                    }

                    // Dodaj godinu u program ako ima predmeta
                    if (statistikaGodina.Statistika.Any())
                    {
                        groupedViewModel.GodineStudija.Add(statistikaGodina);
                    }
                }

                // Dodaj program u rezultat ako ima barem jednu godinu
                if (groupedViewModel.GodineStudija.Any())
                {
                    result.Add(groupedViewModel);
                }
            }

            return View(result);
        }
    }
}
