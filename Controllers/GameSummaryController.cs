using GameStoreWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Text;
using System.Xml.Linq;

namespace GameStoreWeb.Controllers
{
    public class GameSummaryController : Controller
    {
        private string url = "http://localhost:5113/games/";
        private HttpClient client = new HttpClient();

        [HttpGet]
        public IActionResult Index()
        {
            List<GameSummary> games = new List<GameSummary>();
            HttpResponseMessage response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                string result = response.Content.ReadAsStringAsync().Result;
                var data = JsonConvert.DeserializeObject<List<GameSummary>>(result);
                if(data != null)
                {
                    games = data;
                }
            }
            return View(games);
        }

        [HttpGet]
        public IActionResult Create()
        {
            GameStoreViewModel gameStoreViewModel = new GameStoreViewModel
            {
                GameDetails = null,
                GameSummary = new GameSummary(),
                GenreModel = BindDDL()
            };
            return View(gameStoreViewModel);
        }

        [HttpPost]
        public IActionResult Create(GameStoreViewModel gameStoreViewModel)
        {
            CreateGameModel createGameModel = new CreateGameModel();
            createGameModel.Name = gameStoreViewModel.GameSummary.name;
            createGameModel.GenreId = gameStoreViewModel.GenreModel.Id;
            createGameModel.Price = gameStoreViewModel.GameSummary.price;
            createGameModel.ReleaseDate = gameStoreViewModel.GameSummary.releaseDate.ToString("yyyy-MM-dd");

            string gameSummaryJson = JsonConvert.SerializeObject(createGameModel);

            StringContent content = new StringContent(gameSummaryJson, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                TempData["inserted_message"] = "Game Added successfully";
                return RedirectToAction("Index");
            }

            gameStoreViewModel.GenreModel = BindDDL();

            return View(gameStoreViewModel);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            GameDetails gameDetails = new GameDetails();
            HttpResponseMessage response = client.GetAsync(url + id).Result;
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
                GenreModel = BindDDL(),
            };

            return View(gameStoreViewModel);
        }

        [HttpPost]
        public IActionResult Edit(GameStoreViewModel gameStoreViewModel, int Id)
        {
            UpdateGameModel updateGameModel = new UpdateGameModel();
            updateGameModel.Name = gameStoreViewModel.GameDetails.Name;
            updateGameModel.GenreId = gameStoreViewModel.GameDetails.GenreId;
            updateGameModel.Price = gameStoreViewModel.GameDetails.Price;
            updateGameModel.ReleaseDate = gameStoreViewModel.GameDetails.ReleaseDate.ToString("yyyy-MM-dd");

            string gameSummaryJson = JsonConvert.SerializeObject(updateGameModel);

            StringContent content = new StringContent(gameSummaryJson, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PutAsync(url + Id, content).Result;
            if (response.IsSuccessStatusCode)
            {
                TempData["updated_message"] = "Game Updated successfully";
                return RedirectToAction("Index");
            }

            gameStoreViewModel.GenreModel = BindDDL();

            return View(gameStoreViewModel);
        }

        [HttpGet]
        public IActionResult Details(int Id)
        {
            GameDetails game = new GameDetails();
            GameSummary summary = new GameSummary();
            HttpResponseMessage response = client.GetAsync(url + Id).Result;
            if (response.IsSuccessStatusCode)
            {
                string result = response.Content.ReadAsStringAsync().Result;
                var data = JsonConvert.DeserializeObject<GameDetails>(result);                
                if (data != null)
                {
                    game = data;
                }
                GenreModel genreModel = BindDDL();
                summary.id = game.Id;
                summary.name = game.Name;
                summary.genre = genreModel.Genres?.Find(x => x.Value == game.GenreId.ToString())?.Text;
                summary.price = game.Price;
                summary.releaseDate = game.ReleaseDate;
                return View(summary);
            }
            TempData["alert_message"] = "Game not found";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(int Id)
        {
            GameDetails game = new GameDetails();
            GameSummary summary = new GameSummary();
            HttpResponseMessage response = client.GetAsync(url + Id).Result;
            if (response.IsSuccessStatusCode)
            {
                string result = response.Content.ReadAsStringAsync().Result;
                var data = JsonConvert.DeserializeObject<GameDetails>(result);
                if (data != null)
                {
                    game = data;
                }
                GenreModel genreModel = BindDDL();
                summary.id = game.Id;
                summary.name = game.Name;
                summary.genre = genreModel.Genres?.Find(x => x.Value == game.GenreId.ToString())?.Text;
                summary.price = game.Price;
                summary.releaseDate = game.ReleaseDate;
                return View(summary);
            }
            TempData["alert_message"] = "Game not found";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(GameSummary gameSummary, int Id)
        {
            HttpResponseMessage response = client.DeleteAsync(url + Id).Result;
            if (response.IsSuccessStatusCode)
            {
                TempData["delete_message"] = "Game Deleted successfully";
                return RedirectToAction("Index");
            }
            return View(gameSummary);
        }

        private GenreModel BindDDL()
        {
            GenreModel genreModel = new GenreModel();
            genreModel.Genres = new List<SelectListItem>();
            HttpResponseMessage genreResponse = client.GetAsync("http://localhost:5113/genres").Result;
            if (genreResponse.IsSuccessStatusCode)
            {
                string genreResult = genreResponse.Content.ReadAsStringAsync().Result;
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
    }
}
