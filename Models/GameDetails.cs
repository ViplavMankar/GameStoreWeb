using System.ComponentModel.DataAnnotations;

namespace GameStoreWeb.Models
{
    public class GameDetails
    {
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
        [Required]
        public int GenreId { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        [DataType(DataType.Date)]
        public DateOnly ReleaseDate { get; set; } = new DateOnly(2000, 1, 1);
    }
}
