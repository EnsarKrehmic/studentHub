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
        [HttpGet]
        [Route("")]
        [Route("[Controller]/[Action]")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Uvjerenja
                .Include(u => u.Student)
                .Include(u => u.StudentskaSluzba);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Uvjerenja/Details/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

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
        [HttpGet]
        [Route("[Controller]/[Action]")]
        public IActionResult Create()
        {
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "Ime");
            ViewData["StudentskaSluzbaId"] = new SelectList(_context.StudentskeSluzbe, "Id", "Ime");
            return View();
        }

        // POST: Uvjerenja/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("[Controller]/[Action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Namjena,datumIzdavanja,brojIndeksa,StudentId,StudentskaSluzbaId,Vrsta")] Uvjerenje uvjerenje)
        {
            if (ModelState.IsValid)
            {
                _context.Add(uvjerenje);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "Ime", uvjerenje.StudentId);
            ViewData["StudentskaSluzbaId"] = new SelectList(_context.StudentskeSluzbe, "Id", "Ime", uvjerenje.StudentskaSluzbaId);
            return View(uvjerenje);
        }

        // GET: Uvjerenja/Edit/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uvjerenje = await _context.Uvjerenja.FindAsync(id);
            if (uvjerenje == null)
            {
                return NotFound();
            }
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "Ime", uvjerenje.StudentId);
            ViewData["StudentskaSluzbaId"] = new SelectList(_context.StudentskeSluzbe, "Id", "Ime", uvjerenje.StudentskaSluzbaId);
            return View(uvjerenje);
        }

        // POST: Uvjerenja/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Namjena,datumIzdavanja,brojIndeksa,StudentId,StudentskaSluzbaId,Vrsta")] Uvjerenje uvjerenje)
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
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "Ime", uvjerenje.StudentId);
            ViewData["StudentskaSluzbaId"] = new SelectList(_context.StudentskeSluzbe, "Id", "Ime", uvjerenje.StudentskaSluzbaId);
            return View(uvjerenje);
        }

        // GET: Uvjerenja/Delete/5
        [HttpGet]
        [Route("[Controller]/[Action]/{id?}")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

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

        // POST: Uvjerenja/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("[Controller]/[Action]/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var uvjerenje = await _context.Uvjerenja.FindAsync(id);
            if (uvjerenje != null)
            {
                _context.Uvjerenja.Remove(uvjerenje);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UvjerenjeExists(long id)
        {
            return _context.Uvjerenja.Any(e => e.Id == id);
        }
    }
}
