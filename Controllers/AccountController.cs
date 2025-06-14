using Microsoft.AspNetCore.Mvc;
using GameStoreWeb.Models;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace GameStoreWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient("AuthService");
            var response = await client.PostAsJsonAsync("api/auth/login", model);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Invalid login.");
                TempData["ErrorMessage"] = "Invalid username or password.";
                return View(model);
            }

            var result = await response.Content.ReadFromJsonAsync<AuthResponseModel>();
            // Store JWT in cookie or local storage (session/cookie setup follows)
            HttpContext.Session.SetString("JWToken", result.Token);
            HttpContext.Session.SetString("Username", model.Username);

            HttpContext.Response.Cookies.Append("JWToken", result.Token, new CookieOptions
            {
                HttpOnly = false, // set to true if you're not accessing via JS
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddMinutes(30)
            });


            // ✅ NEW: Sign in using cookies
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.Username)
                // optionally add roles etc.
            };

            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync("Cookies", claimsPrincipal); // this is key
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient("AuthService");
            var response = await client.PostAsJsonAsync("api/auth/register", model);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Registration failed.");
                TempData["RegistrationErrorMessage"] = "Registration failed. Please try again.";
                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    ModelState.AddModelError("Username", "Username already exists.");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    ModelState.AddModelError("", "Invalid registration data.");
                }
                return View(model);
            }
            TempData["Registration_success_message"] = "Account created successfully, Please login.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
