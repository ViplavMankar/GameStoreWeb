using System.ComponentModel.DataAnnotations;

namespace GameStoreWeb.Models
{
    public class GameSummary
    {
        public int id { get; set; }
        [Required]
        public string? name { get; set; }
        [Required]
        public string? genre { get; set; }
        [Required]
        public decimal price { get; set; }
        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        [DataType(DataType.Date)]
        public DateOnly releaseDate { get; set; } = new DateOnly(2000, 1, 1);

        //gamesummary -> gamesummary on UI as well
    }
}
