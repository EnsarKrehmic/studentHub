using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace StudentHub.Areas.Identity.Pages.Account
{
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ResendEmailConfirmationModel> _logger;

        public ResendEmailConfirmationModel(UserManager<IdentityUser> userManager, ILogger<ResendEmailConfirmationModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Email adresa je obavezna.")]
            [EmailAddress(ErrorMessage = "Neispravan format email adrese.")]
            public string Email { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                // Ne otkrivamo da li korisnik postoji ili ne
                return RedirectToPage("./ResendEmailConfirmationConfirmation");
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, code = token },
                protocol: Request.Scheme);

            // Ovdje ide tvoja logika za slanje emaila sa linkom
            _logger.LogInformation("Email confirmation link (for debug): {CallbackUrl}", callbackUrl);

            // TODO: Umjesto logiranja, ovdje pozovi svoj servis za email i pošalji korisniku link!

            return RedirectToPage("./ResendEmailConfirmationConfirmation");
        }
    }
}
