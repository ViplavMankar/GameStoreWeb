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
        private readonly IConfiguration _configuration;

        public AccountController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (Environment.GetEnvironmentVariable("RENDER") != null)
            {
                ViewBag.AuthServiceBaseUrl = Environment.GetEnvironmentVariable("AUTH_SERVICE_URL");
            }
            else
            {
                ViewBag.AuthServiceBaseUrl = _configuration["AuthService:BaseUrl"];
            }
            return View();
        }

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
        public IActionResult Register()
        {
            if (Environment.GetEnvironmentVariable("RENDER") != null)
            {
                ViewBag.AuthServiceBaseUrl = Environment.GetEnvironmentVariable("AUTH_SERVICE_URL");
            }
            else
            {
                ViewBag.AuthServiceBaseUrl = _configuration["AuthService:BaseUrl"];
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient("AuthService");
            var response = await client.PostAsJsonAsync("api/auth/register", model);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Registration failed.");
                TempData["Registration_error_message"] = "Registration failed. Please try again.";
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

        [HttpGet]
        public async Task<IActionResult> Callback(string token, string refreshToken)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Google login failed. No token received.";
                return RedirectToAction("Login");
            }
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var username = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                            ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value
                            ?? "GoogleUser";
            // 1. Store JWT + Refresh Token in session for API calls
            HttpContext.Session.SetString("JWToken", token);
            HttpContext.Session.SetString("RefreshToken", refreshToken);
            HttpContext.Session.SetString("Username", username);

            // 2. Store JWT as a cookie (for Blazor or JS access)
            HttpContext.Response.Cookies.Append("JWToken", token, new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddMinutes(30)
            });

            // 3. Sign in user to MVC using cookie auth (so [Authorize] works)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username)
                // optionally you can add role claims here
            };
            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync("Cookies", claimsPrincipal);

            // Redirect to home or wherever you want
            return RedirectToAction("Index", "Home");
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
