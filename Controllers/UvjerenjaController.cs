using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;

namespace StudentHub.Controllers
{
    [Route("Uvjerenja")]
    public class UvjerenjaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UvjerenjaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Uvjerenja
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var uvjerenja = await _context.Uvjerenja.Include(u => u.Student).ToListAsync();
            return View(uvjerenja);
        }

        // GET: Uvjerenja/Details/{id}
        [HttpGet("Details/{id:long}")]
        public async Task<IActionResult> Details(long id)
        {
            var uvjerenje = await _context.Uvjerenja
                .Include(u => u.Student)
                .Include(u => u.StudentskaSluzba)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (uvjerenje == null)
            {
                return NotFound();
            }

            return View(uvjerenje);
        }

        // GET: Uvjerenja/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            ViewBag.Studenti = new SelectList(_context.Studenti, "Id", "ImePrezime");
            return View();
        }

        // POST: Uvjerenja/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Namjena,DatumIzdavanja,StudentId,Vrsta")] Uvjerenje uvjerenje)
        {
            if (ModelState.IsValid)
            {
                _context.Uvjerenja.Add(uvjerenje);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Studenti = new SelectList(_context.Studenti, "Id", "ImePrezime", uvjerenje.StudentId);
            return View(uvjerenje);
        }

        // GET: Uvjerenja/Edit/{id}
        [HttpGet("Edit/{id:long}")]
        public async Task<IActionResult> Edit(long id)
        {
            var uvjerenje = await _context.Uvjerenja.FindAsync(id);
            if (uvjerenje == null)
            {
                return NotFound();
            }
            ViewBag.Studenti = new SelectList(_context.Studenti, "Id", "ImePrezime", uvjerenje.StudentId);
            return View(uvjerenje);
        }

        // POST: Uvjerenja/Edit/{id}
        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Namjena,DatumIzdavanja,StudentId,Vrsta")] Uvjerenje uvjerenje)
        {
            if (id != uvjerenje.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(uvjerenje);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UvjerenjeExists(uvjerenje.Id))
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
            ViewBag.Studenti = new SelectList(_context.Studenti, "Id", "ImePrezime", uvjerenje.StudentId);
            return View(uvjerenje);
        }

        // GET: Uvjerenja/Delete/{id}
        [HttpGet("Delete/{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var uvjerenje = await _context.Uvjerenja
                .Include(u => u.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (uvjerenje == null)
            {
                return NotFound();
            }

            return View(uvjerenje);
        }

        // POST: Uvjerenja/Delete/{id}
        [HttpPost("Delete/{id:long}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var uvjerenje = await _context.Uvjerenja.FindAsync(id);
            if (uvjerenje != null)
            {
                _context.Uvjerenja.Remove(uvjerenje);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool UvjerenjeExists(long id)
        {
            return _context.Uvjerenja.Any(e => e.Id == id);
        }
    }
}
