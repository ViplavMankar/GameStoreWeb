using System;

namespace GameStoreWeb.DTOs;

public class AchievementUnlockedDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}
