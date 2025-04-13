using GameStoreWeb;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddServerSideBlazor();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // adjust as needed
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Cookies";
    options.DefaultChallengeScheme = "Cookies";
})
.AddCookie("Cookies", options =>
{
    options.LoginPath = new PathString("/Account/Login");
    options.AccessDeniedPath = new PathString("/Account/AccessDenied");
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("gamestore_app_authentication_jwt_key")) // <- pull from config!
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // ✅ Grab JWT from session instead of Authorization header
            var token = context.HttpContext.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
                context.Token = token;

            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            context.Response.Redirect("/Account/Login");
            context.HandleResponse(); // Prevents the default 401 response
            return Task.CompletedTask;
        }
    };
});

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpClient("AuthService", client =>
    {
        client.BaseAddress = new Uri("http://localhost:5204/");
    });
}
else if (Environment.GetEnvironmentVariable("RENDER") != null) // Only on Render
{
    builder.Services.AddHttpClient("AuthService", client =>
    {
        client.BaseAddress = Environment.GetEnvironmentVariable("AUTH_SERVICE_URL") != null
            ? new Uri(Environment.GetEnvironmentVariable("AUTH_SERVICE_URL"))
            : new Uri("https://auth-service-production-1234.onrender.com/");
    });
}

if (Environment.GetEnvironmentVariable("RENDER") != null) // Only on Render
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

}
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
if (Environment.GetEnvironmentVariable("RENDER") != null) // Only on Render
{
    app.MapGet("/health", () => Results.Ok("API is running"));
}
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=GameSummary}/{action=Index}/{id?}");

app.MapBlazorHub();
app.Run();
