using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GameStoreWeb.Models;
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

    public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public IActionResult Index()
    {
        var client = _httpClientFactory.CreateClient("GameStoreApiService");
        try
        {
            AttachTokenToClient(client);
        }
        catch
        {
            return RedirectToAction("Login", "Account");
        }
        return View();
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
    private void AttachTokenToClient(HttpClient client)
    {
        var token = HttpContext.Session.GetString("JWToken");
        if (string.IsNullOrEmpty(token))
            throw new UnauthorizedAccessException("No JWT token in session");

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    private async Task CheckApiHealth(HttpClient client)
    {
        bool apiAwake = await WakeUpApi(client, "health");
        if (!apiAwake)
        {
            Console.WriteLine("⚠️ API did not respond to wake-up request.");
        }
        else
        {
            Console.WriteLine("✅ API is awake.");
        }
    }
    private static async Task<bool> WakeUpApi(HttpClient client, string healthCheckUrl)
    {
        try
        {
            Console.WriteLine("🔄 Waking up API...");
            HttpResponseMessage response = await client.GetAsync(healthCheckUrl);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
