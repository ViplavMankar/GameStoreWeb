using GameStoreWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;


namespace GameStoreWeb.Controllers
{
    [Authorize]
    public class GameSummaryController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GameSummaryController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
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
            List<GameSummary> games = new List<GameSummary>();
            await CheckApiHealth(client);

            HttpResponseMessage response = await client.GetAsync("games/");
            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<List<GameSummary>>(result);
                if (data != null)
                {
                    games = data;
                }
            }
            return View(games);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
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

            GameStoreViewModel gameStoreViewModel = new GameStoreViewModel
            {
                GameDetails = null,
                GameSummary = new GameSummary(),
                GenreModel = await BindDDL(client)
            };
            return View(gameStoreViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(GameStoreViewModel gameStoreViewModel)
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

            var createGameModel = new CreateGameModel
            {
                Name = gameStoreViewModel.GameSummary.name,
                GenreId = gameStoreViewModel.GenreModel.Id,
                Price = gameStoreViewModel.GameSummary.price,
                ReleaseDate = gameStoreViewModel.GameSummary.releaseDate.ToString("yyyy-MM-dd")
            };

            var gameSummaryJson = JsonConvert.SerializeObject(createGameModel);
            var content = new StringContent(gameSummaryJson, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("games/", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["inserted_message"] = "Game Added successfully";
                return RedirectToAction("Index");
            }

            // Re-bind dropdown in case of failure
            gameStoreViewModel.GenreModel = await BindDDL(client);

            return View(gameStoreViewModel);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
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

            GameDetails gameDetails = new GameDetails();
            HttpResponseMessage response = await client.GetAsync("games/" + id);
            if (response.IsSuccessStatusCode)
            {
                string result = response.Content.ReadAsStringAsync().Result;
                var data = JsonConvert.DeserializeObject<GameDetails>(result);
                if (data != null)
                {
                    gameDetails = data;
                }
            }

            GameStoreViewModel gameStoreViewModel = new GameStoreViewModel
            {
                GameDetails = gameDetails,
                GameSummary = null,
                GenreModel = await BindDDL(client),
            };

            return View(gameStoreViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(GameStoreViewModel gameStoreViewModel, int Id)
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

            var updateGameModel = new UpdateGameModel
            {
                Name = gameStoreViewModel.GameDetails.Name,
                GenreId = gameStoreViewModel.GameDetails.GenreId,
                Price = gameStoreViewModel.GameDetails.Price,
                ReleaseDate = gameStoreViewModel.GameDetails.ReleaseDate.ToString("yyyy-MM-dd")
            };

            string gameSummaryJson = JsonConvert.SerializeObject(updateGameModel);

            StringContent content = new StringContent(gameSummaryJson, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PutAsync("games/" + Id, content);
            if (response.IsSuccessStatusCode)
            {
                TempData["updated_message"] = "Game Updated successfully";
                return RedirectToAction("Index");
            }

            gameStoreViewModel.GenreModel = await BindDDL(client);

            return View(gameStoreViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int Id)
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

            GameDetails game = new GameDetails();
            HttpResponseMessage response = await client.GetAsync("/games/" + Id);
            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<GameDetails>(result);
                if (data != null)
                {
                    game = data;
                }
                GenreModel genreModel = await BindDDL(client);
                GameSummary summary = new GameSummary
                {
                    id = game.Id,
                    name = game.Name,
                    genre = genreModel.Genres?.Find(x => x.Value == game.GenreId.ToString())?.Text,
                    price = game.Price,
                    releaseDate = game.ReleaseDate
                };
                return View(summary);
            }
            TempData["alert_message"] = "Game not found";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int Id)
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

            GameDetails game = new GameDetails();
            HttpResponseMessage response = await client.GetAsync("games/" + Id);
            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<GameDetails>(result);
                if (data != null)
                {
                    game = data;
                }
                GenreModel genreModel = await BindDDL(client);
                GameSummary summary = new GameSummary
                {
                    id = game.Id,
                    name = game.Name,
                    genre = genreModel.Genres?.Find(x => x.Value == game.GenreId.ToString())?.Text,
                    price = game.Price,
                    releaseDate = game.ReleaseDate
                };
                return View(summary);
            }
            TempData["alert_message"] = "Game not found";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(GameSummary gameSummary, int Id)
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

            HttpResponseMessage response = await client.DeleteAsync("games/" + Id);
            if (response.IsSuccessStatusCode)
            {
                TempData["delete_message"] = "Game Deleted successfully";
                return RedirectToAction("Index");
            }
            return View(gameSummary);
        }

        private async Task<GenreModel> BindDDL(HttpClient client)
        {
            GenreModel genreModel = new GenreModel();
            genreModel.Genres = new List<SelectListItem>();
            HttpResponseMessage genreResponse = await client.GetAsync("genres");
            if (genreResponse.IsSuccessStatusCode)
            {
                string genreResult = await genreResponse.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<List<Genre>>(genreResult);
                genreModel.Genres.Add(new SelectListItem
                {
                    Text = "Select Genre",
                    Value = ""
                });
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        genreModel.Genres.Add(new SelectListItem
                        {
                            Text = item.name,
                            Value = item.id.ToString()
                        });
                    }
                }
            }
            return genreModel;
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

        //private void RetryCode()
        //{
        // int maxRetries = 3;
        // int delay = 2000; // 2 seconds
        // for (int i = 0; i < maxRetries; i++)
        // {
        //     try
        //     {
        //     }
        //     catch (HttpRequestException)
        //     {
        //         Console.WriteLine($"Retrying in {delay / 1000} seconds...");
        //     }

        //     await Task.Delay(delay);
        //     delay *= 2; // Exponential backoff
        // }
        // RedirectToAction("Error");
        //}
    }
}
