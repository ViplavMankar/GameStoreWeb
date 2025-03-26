﻿namespace GameStoreWeb.Models
{
    public class CreateGameModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int GenreId { get; set; }
        public decimal Price { get; set; }
        public string ReleaseDate { get; set; }
    }
}
