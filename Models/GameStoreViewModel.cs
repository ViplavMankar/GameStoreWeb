using Microsoft.AspNetCore.Mvc.Rendering;

namespace GameStoreWeb.Models
{
    public class GameStoreViewModel
    {
        public GameSummary? GameSummary { get; set; }
        public GenreModel? GenreModel { get; set; }
        public GameDetails? GameDetails { get; set; }
    }
}
