using System;

namespace GameStoreWeb.DTOs;

public class ChallengeDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TargetValue { get; set; }
    public int XPReward { get; set; }
    public int CurrentProgress { get; set; }
    public bool IsCompleted { get; set; }
}
