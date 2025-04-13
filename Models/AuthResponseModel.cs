namespace GameStoreWeb.Models
{
    public class AuthResponseModel
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}