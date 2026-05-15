using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GameStoreWeb.Hubs;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class RealtimeHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // Console.WriteLine("SignalR Connected");

        // Console.WriteLine($"UserIdentifier: {Context.UserIdentifier}");

        // Console.WriteLine($"NameIdentifier Claim: " +
        //     Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        await base.OnConnectedAsync();
    }
}
