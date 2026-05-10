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

namespace GameStoreWeb.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDashboardService _dashboardService;

    public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, IDashboardService dashboardService)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _dashboardService = dashboardService;
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
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // if (string.IsNullOrEmpty(userIdClaim))
            // {
            //     _logger.LogWarning("UserId claim missing. User not authenticated properly.");
            //     return RedirectToAction("Login", "Account");
            // }

            var userId = Guid.Parse(userIdClaim);
            // AttachTokenToClient(client);

            var model = new DashboardViewModel();

            model.TotalXP = await _dashboardService.GetXP(userId);

            var (current, max) = await _dashboardService.GetStreak(userId);
            model.CurrentStreak = current;
            model.MaxStreak = max;

            return View(model);
            // var model = new DashboardViewModel();
            // model.TotalXP = 0;
            // model.CurrentStreak = 0;
            // model.MaxStreak = 0;
            // // var xp = await client.GetFromJsonAsync<XPViewModel>("api/dashboards/GetXP");
            // // var streak = await client.GetFromJsonAsync<StreakViewModel>("api/dashboards/GetStreak");
            // model.TotalXP = xp?.TotalXP ?? 0;
            // model.CurrentStreak = streak?.CurrentStreak ?? 0;
            // model.MaxStreak = streak?.MaxStreak ?? 0;
            // return View(model);
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
