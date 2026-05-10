using System;
using GameStoreWeb.DTOs;

namespace GameStoreWeb.Services;

public interface IProfileService
{
    Task<PlayerProfileDto> GetPlayerProfileAsync(Guid userId);
}
