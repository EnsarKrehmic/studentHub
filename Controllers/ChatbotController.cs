using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
using StudentHub.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StudentHub.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatbotController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ChatbotController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("Ask")]
        public async Task<IActionResult> Ask([FromBody] ChatbotRequestVM model)
        {
            var question = model.Question?.Trim();
            if (string.IsNullOrWhiteSpace(question))
                return BadRequest(new ChatbotResponseVM { AnswerFound = false, Answer = "Nije poslano pitanje." });

            // 1. Pretraga FAQ
            var faqMatch = await _context.FaqPitanja
                .OrderByDescending(f => f.Preporuceno)
                .FirstOrDefaultAsync(f =>
                    EF.Functions.Like(f.Pitanje.ToLower(), $"%{question.ToLower()}%") ||
                    EF.Functions.Like(f.Odgovor.ToLower(), $"%{question.ToLower()}%")
                );
            if (faqMatch != null)
            {
                return Ok(new ChatbotResponseVM
                {
                    AnswerFound = true,
                    Answer = faqMatch.Odgovor,
                    Source = "FAQ: " + faqMatch.Pitanje
                });
            }

            // 2. Pretraga KnowledgeSnippets
            var snippetMatch = await _context.KnowledgeSnippets
                .OrderByDescending(s => s.DatumDodavanja)
                .FirstOrDefaultAsync(s =>
                    EF.Functions.Like(s.Naslov.ToLower(), $"%{question.ToLower()}%") ||
                    EF.Functions.Like(s.Sadrzaj.ToLower(), $"%{question.ToLower()}%")
                );
            if (snippetMatch != null)
            {
                return Ok(new ChatbotResponseVM
                {
                    AnswerFound = true,
                    Answer = snippetMatch.Sadrzaj,
                    Source = snippetMatch.Izvor + ": " + snippetMatch.Naslov
                });
            }

            // 3. Loguj upit za studentsku službu (i kreiraj PodrskaUpit)
            long? korisnikId = null;
            if (User.Identity.IsAuthenticated)
            {
                var userAspId = User.Claims.FirstOrDefault(x => x.Type.Contains("nameidentifier"))?.Value;
                var korisnik = await _context.Korisnici.FirstOrDefaultAsync(u => u.AspNetUserId == userAspId);
                if (korisnik != null)
                    korisnikId = korisnik.Id;
            }

            var podrskaUpit = new PodrskaUpit
            {
                Naslov = "AI Chatbot upit: " + (question.Length > 50 ? question.Substring(0, 50) + "..." : question),
                Opis = question,
                DatumKreiranja = DateTime.Now,
                Status = UpitStatus.Podnesen,
                KorisnikId = korisnikId ?? 0
            };
            _context.PodrskaUpiti.Add(podrskaUpit);
            await _context.SaveChangesAsync();

            var upit = new ChatbotLogUpit
            {
                Pitanje = question,
                Datum = DateTime.Now,
                Status = "Čeka odgovor službe",
                UserId = korisnikId,
                PodrskaUpitId = podrskaUpit.Id
            };
            _context.ChatbotLogUpiti.Add(upit);
            await _context.SaveChangesAsync();

            return Ok(new ChatbotResponseVM
            {
                AnswerFound = false,
                Answer = "Nažalost, ne mogu pronaći odgovor u bazi. Vaš upit je proslijeđen studentskoj službi i dobit ćete odgovor uskoro.",
                Source = ""
            });
        }
    }
}
