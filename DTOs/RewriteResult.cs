using System;

namespace GameStoreWeb.DTOs;

public class RewriteResult
{
    public required string Markdown { get; init; }
    public string? Title { get; init; }
    public string? Summary { get; init; }
    public int EstimatedWordCount { get; init; }
}
