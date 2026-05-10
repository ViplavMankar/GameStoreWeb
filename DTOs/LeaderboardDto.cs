using System;

namespace GameStoreWeb.DTOs;

public class LeaderboardDto
{
    public int Rank { get; set; }
    public string Username { get; set; }
    public int Value { get; set; }
}
