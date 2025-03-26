using Microsoft.AspNetCore.Mvc.Rendering;

namespace GameStoreWeb.Models
{
    public class GenreModel
    {
        public int Id { get; set; }
        public List<SelectListItem>? Genres{ get; set; }
    }
}
