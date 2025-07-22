
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Components.Authorization;

using GameStoreWeb;
using GameStoreWeb.Auth;

var builder = WebApplication.CreateBuilder(args);
var jwtKey = string.Empty;

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

if (builder.Environment.IsDevelopment())
{
    jwtKey = builder.Configuration["Jwt:Key"];
    if (string.IsNullOrEmpty(jwtKey))
    {
        throw new InvalidOperationException("jwtkey appsetting value not set.");
    }
}
else if (Environment.GetEnvironmentVariable("RENDER") != null) // Only on Render
{
    jwtKey = Environment.GetEnvironmentVariable("JWT_AUTH_KEY");
    if (string.IsNullOrEmpty(jwtKey))
    {
        throw new InvalidOperationException("JWT_AUTH_KEY environment variable is not set.");
    }
}

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
    // options.AccessDeniedPath = new PathString("/Account/AccessDenied");
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
            Encoding.UTF8.GetBytes(jwtKey)) // <- pull from config!
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

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor(); // Needed to access session inside the provider
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpClient("AuthService", client =>
    {
        client.BaseAddress = new Uri("http://localhost:5208/");
    });
    builder.Services.AddHttpClient("GameStoreApiService", client =>
    {
        client.BaseAddress = new Uri("http://localhost:5113/");
    });
}
else if (Environment.GetEnvironmentVariable("RENDER") != null) // Only on Render
{
    builder.Services.AddHttpClient("AuthService", client =>
    {
        client.BaseAddress = Environment.GetEnvironmentVariable("AUTH_SERVICE_URL") != null
            ? new Uri(Environment.GetEnvironmentVariable("AUTH_SERVICE_URL"))
            : throw new Exception("Environment variable not set.");
    });
    builder.Services.AddHttpClient("GameStoreApiService", client =>
    {
        client.BaseAddress = Environment.GetEnvironmentVariable("GAMESTORE_API_URL") != null
            ? new Uri(Environment.GetEnvironmentVariable("GAMESTORE_API_URL"))
            : throw new Exception("Environment variable not set.");
    });
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
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
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapBlazorHub();

app.MapFallbackToPage("/marketplace", "/_Host");
app.MapFallbackToPage("/addGame", "/_Host");
app.Run();