using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;

namespace StudentHub.Controllers
{
    [Route("Korisnici")]
    public class KorisniciController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KorisniciController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Korisnici
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Korisnici.ToListAsync());
        }

        // GET: Korisnici/Details/{id}
        [HttpGet("Details/{id:long}")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var korisnik = await _context.Korisnici
                .FirstOrDefaultAsync(m => m.Id == id);
            if (korisnik == null)
            {
                return NotFound();
            }

            return View(korisnik);
        }

        // GET: Korisnici/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Korisnici/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,JMBG,Ime,Prezime,Email,Lozinka,Uloga")] Korisnik korisnik)
        {
            if (ModelState.IsValid)
            {
                _context.Add(korisnik);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(korisnik);
        }

        // GET: Korisnici/Edit/{id}
        [HttpGet("Edit/{id:long}")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var korisnik = await _context.Korisnici.FindAsync(id);
            if (korisnik == null)
            {
                return NotFound();
            }
            return View(korisnik);
        }

        // POST: Korisnici/Edit/{id}
        [HttpPost("Edit/{id:long}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,JMBG,Ime,Prezime,Email,Lozinka,Uloga")] Korisnik korisnik)
        {
            if (id != korisnik.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(korisnik);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KorisnikExists(korisnik.Id))
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
            return View(korisnik);
        }

        // GET: Korisnici/Delete/{id}
        [HttpGet("Delete/{id:long}")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var korisnik = await _context.Korisnici
                .FirstOrDefaultAsync(m => m.Id == id);
            if (korisnik == null)
            {
                return NotFound();
            }

            return View(korisnik);
        }

        // POST: Korisnici/Delete/{id}
        [HttpPost("Delete/{id:long}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var korisnik = await _context.Korisnici.FindAsync(id);
            if (korisnik != null)
            {
                _context.Korisnici.Remove(korisnik);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KorisnikExists(long id)
        {
            return _context.Korisnici.Any(e => e.Id == id);
        }

        // GET: Korisnici/Profile/{id}
        [HttpGet("Profile/{id:long}")]
        public async Task<IActionResult> Profile(long id)
        {
            var korisnik = await _context.Korisnici
                .Include(k => k.Student)
                .Include(k => k.Profesor)
                .Include(k => k.Asistent)
                .Include(k => k.StudentskaSluzba)
                .FirstOrDefaultAsync(k => k.Id == id);

            if (korisnik == null)
            {
                return NotFound();
            }

            var dokumenti = await _context.Dokumenti
                .Where(d => d.StudentId == korisnik.StudentId || d.StudentskaSluzbaId == korisnik.Id)
                .ToListAsync();

            var zahtjevi = await _context.Zahtjevi
                .Where(z => z.StudentId == korisnik.StudentId)
                .ToListAsync();

            var uvjerenja = await _context.Uvjerenja
                .Where(u => u.StudentId == korisnik.StudentId)
                .ToListAsync();

            var viewModel = new KorisnikProfileViewModel
            {
                Id = korisnik.Id,
                JMBG = korisnik.JMBG,
                Ime = korisnik.Ime,
                Prezime = korisnik.Prezime,
                Email = korisnik.Email,
                Uloga = korisnik.Uloga,
                Student = korisnik.Student,
                Profesor = korisnik.Profesor,
                Asistent = korisnik.Asistent,
                StudentskaSluzba = korisnik.StudentskaSluzba,
                Dokumenti = dokumenti,
                Zahtjevi = zahtjevi,
                Uvjerenja = uvjerenja
            };

            return View(viewModel);
        }
    }
}
