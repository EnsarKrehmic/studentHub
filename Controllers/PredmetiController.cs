using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;
using System.Linq;

namespace StudentHub.Controllers
{
    [Route("Predmeti")]
    public class PredmetController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PredmetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Predmet/Index
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var predmeti = _context.Predmeti
                    .Include(p => p.Profesor)
                    .Include(p => p.Asistent)
                    .ToList();
                if (!predmeti.Any())
                {
                    Console.WriteLine("Nema predmeta.");
                }
                return View(predmeti);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return View("Error");
            }
        }


        // GET: Predmet/Details/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public IActionResult Details(long id)
        {
            var predmet = _context.Predmeti
                .Include(p => p.Profesor)
                .Include(p => p.Asistent)
                .FirstOrDefault(p => p.Id == id);

            if (predmet == null)
                return NotFound();

            var profesori = _context.PredmetProfesori
                .Where(pp => pp.PredmetId == id)
                .Include(pp => pp.Profesor)
                .ToList();

            var asistenti = _context.PredmetAsistenti
                .Where(pa => pa.PredmetId == id)
                .Include(pa => pa.Asistent)
                .ToList();

            var viewModel = new PredmetDetailsViewModel
            {
                Predmet = predmet,
                Profesori = profesori,
                Asistenti = asistenti
            };

            return View(viewModel);
        }

        // GET: Predmet/Create
        [HttpGet]
        [Route("[Controller]/[Action]")]
        public IActionResult Create()
        {
            ViewBag.Profesori = _context.Profesori.ToList();
            ViewBag.Asistenti = _context.Asistenti.ToList();
            ViewBag.NastavniPlanId = _context.NastavniPlanovi
                .Select(np => new SelectListItem
                {
                    Value = np.Id.ToString(),
                    Text = np.GodinaStudija.ToString()
                })
                .ToList();
            return View();
        }

        // POST: Predmet/Create
        [HttpPost]
        [Route("[Controller]/[Action]")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PredmetCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Profesori = _context.Profesori.ToList();
                ViewBag.Asistenti = _context.Asistenti.ToList();
                ViewBag.NastavniPlanId = _context.NastavniPlanovi
                    .Select(np => new SelectListItem
                    {
                        Value = np.Id.ToString(),
                        Text = np.GodinaStudija.ToString()
                    })
                    .ToList();
                return View(model);
            }

            var predmet = new Predmet
            {
                Naziv = model.Naziv,
                Opis = model.Opis,
                ECTS = model.ECTS,
                ProfesorId = model.ProfesorId,
                AsistentId = model.AsistentId,
                NastavniPlanId = model.NastavniPlanId.GetValueOrDefault()
            };

            _context.Predmeti.Add(predmet);
            _context.SaveChanges();

            foreach (var profesorId in model.ProfesorIds)
            {
                _context.PredmetProfesori.Add(new PredmetProfesor
                {
                    PredmetId = predmet.Id,
                    ProfesorId = profesorId
                });
            }

            foreach (var asistentId in model.AsistentIds)
            {
                _context.PredmetAsistenti.Add(new PredmetAsistent
                {
                    PredmetId = predmet.Id,
                    AsistentId = asistentId
                });
            }

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Predmet/Edit/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public IActionResult Edit(long id)
        {
            var predmet = _context.Predmeti.Find(id);
            if (predmet == null)
                return NotFound();

            var model = new PredmetCreateViewModel
            {
                Naziv = predmet.Naziv,
                Opis = predmet.Opis,
                ECTS = predmet.ECTS,
                ProfesorId = predmet.ProfesorId,
                AsistentId = predmet.AsistentId,
                ProfesorIds = _context.PredmetProfesori
                    .Where(pp => pp.PredmetId == id)
                    .Select(pp => pp.ProfesorId)
                    .ToList(),
                AsistentIds = _context.PredmetAsistenti
                    .Where(pa => pa.PredmetId == id)
                    .Select(pa => pa.AsistentId)
                    .ToList()
            };

            ViewBag.Profesori = _context.Profesori.ToList();
            ViewBag.Asistenti = _context.Asistenti.ToList();
            return View(model);
        }

        // POST: Predmet/Edit/5
        [HttpPost]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(long id, PredmetCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Profesori = _context.Profesori.ToList();
                ViewBag.Asistenti = _context.Asistenti.ToList();
                return View(model);
            }

            var predmet = _context.Predmeti.Find(id);
            if (predmet == null)
                return NotFound();

            predmet.Naziv = model.Naziv;
            predmet.Opis = model.Opis;
            predmet.ECTS = model.ECTS;
            predmet.ProfesorId = model.ProfesorId;
            predmet.AsistentId = model.AsistentId;

            // Ažuriranje posrednih tabela
            _context.PredmetProfesori.RemoveRange(_context.PredmetProfesori.Where(pp => pp.PredmetId == id));
            _context.PredmetAsistenti.RemoveRange(_context.PredmetAsistenti.Where(pa => pa.PredmetId == id));

            foreach (var profesorId in model.ProfesorIds)
            {
                _context.PredmetProfesori.Add(new PredmetProfesor
                {
                    PredmetId = id,
                    ProfesorId = profesorId
                });
            }

            foreach (var asistentId in model.AsistentIds)
            {
                _context.PredmetAsistenti.Add(new PredmetAsistent
                {
                    PredmetId = id,
                    AsistentId = asistentId
                });
            }

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Predmet/Delete/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public IActionResult Delete(long id)
        {
            var predmet = _context.Predmeti.Find(id);
            if (predmet == null)
                return NotFound();

            return View(predmet);
        }

        // POST: Predmet/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(long id)
        {
            var predmet = _context.Predmeti.Find(id);
            if (predmet == null)
                return NotFound();

            _context.PredmetProfesori.RemoveRange(_context.PredmetProfesori.Where(pp => pp.PredmetId == id));
            _context.PredmetAsistenti.RemoveRange(_context.PredmetAsistenti.Where(pa => pa.PredmetId == id));
            _context.Predmeti.Remove(predmet);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
