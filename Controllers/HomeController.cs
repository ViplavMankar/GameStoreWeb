using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GameStoreWeb.Models;
using System.Security.Claims;
using GameStoreWeb.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace GameStoreWeb.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDashboardService _dashboardService;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ILogger<HomeController> logger,
                        IHttpClientFactory httpClientFactory,
                        IDashboardService dashboardService,
                        UserManager<ApplicationUser> userManager)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _dashboardService = dashboardService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        // var sessionToken = HttpContext.Session.GetString("JWToken");
        // Console.WriteLine($"[INDEX] Session Token: {sessionToken}");
        // Console.WriteLine("Token attached to client for dashboard API calls.");
        // var client = _httpClientFactory.CreateClient();
        // var baseUrl = $"{Request.Scheme}://{Request.Host}";
        // client.BaseAddress = new Uri(baseUrl);
        try
        {
            // foreach (var claim in User.Claims)
            // {
            //     Console.WriteLine($"CLAIM TYPE: {claim.Type} | VALUE: {claim.Value}");
            // }
            // var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // if (string.IsNullOrEmpty(userIdClaim))
            // {
            //     _logger.LogWarning("UserId claim missing. User not authenticated properly.");
            //     return RedirectToAction("Login", "Account");
            // }

            // var userId = Guid.Parse(userIdClaim);
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = Guid.Parse(user.Id);
            // AttachTokenToClient(client);

            var model = new DashboardViewModel();

            model.TotalXP = await _dashboardService.GetXP(userId);

            var (current, max) = await _dashboardService.GetStreak(userId);
            model.CurrentStreak = current;
            model.MaxStreak = max;

            return View(model);
        }
        catch
        {
            return RedirectToAction("Login", "Account");
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Blazor()
    {
        return View("_Host");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // private void AttachTokenToClient(HttpClient client)
    // {
    //     var token = HttpContext.Session.GetString("JWToken");
    //     if (string.IsNullOrEmpty(token))
    //         throw new UnauthorizedAccessException("No JWT token in session");

    //     client.DefaultRequestHeaders.Authorization =
    //         new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    // }
}
