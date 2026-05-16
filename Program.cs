using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using System.Text;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using QuestPDF.Infrastructure;

using GameStoreWeb.Data;
using GameStoreWeb.Services;
using GameStoreWeb.Auth;
using GameStoreWeb.Hubs;
using GameStoreWeb.DTOs;
using GameStoreWeb.Clients;
using GameStoreWeb.Models;
using GameStoreWeb.Middlewares;
using GameStoreWeb.Payments;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

var jwtKey = string.Empty;
var gameStoreConnectionString = string.Empty;
var configuration = builder.Configuration;
var clientId = string.Empty;
var clientSecret = string.Empty;

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor()
        .AddCircuitOptions(options => { options.DetailedErrors = true; });

builder.Services.Configure<AwsSettings>(options =>
{
    options.Region = builder.Configuration["AWS:Region"]
                      ?? Environment.GetEnvironmentVariable("AWS_REGION");

    options.BucketName = builder.Configuration["AWS:BucketName"]
                      ?? Environment.GetEnvironmentVariable("AWS_BUCKET_NAME");

    options.AccessKey = builder.Configuration["AWS:AccessKey"]
                      ?? Environment.GetEnvironmentVariable("AWS_ACCESS_KEY");

    options.SecretKey = builder.Configuration["AWS:SecretKey"]
                      ?? Environment.GetEnvironmentVariable("AWS_SECRET_KEY");
});

builder.Services.Configure<GeminiOptions>(options =>
{
    options.ApiKey = builder.Configuration["Gemini:ApiKey"]
                      ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");

    options.Model = builder.Configuration["Gemini:Model"]
                      ?? Environment.GetEnvironmentVariable("GEMINI_MODEL");

    var maxTokensStr = builder.Configuration["Gemini:MaxTokens"]
                      ?? Environment.GetEnvironmentVariable("GEMINI_MAX_OUTPUT_TOKENS");
    if (int.TryParse(maxTokensStr, out var maxTokens))
    {
        options.MaxOutputTokens = maxTokens;
    }
    else
    {
        options.MaxOutputTokens = 2048; // default fallback
    }

    var temperatureStr = builder.Configuration["Gemini:Temperature"]
                    ?? Environment.GetEnvironmentVariable("GEMINI_TEMPERATURE");
    if (double.TryParse(temperatureStr, out var temperature))
    {
        options.Temperature = temperature;
    }
    else
    {
        options.Temperature = 0.7; // default fallback
    }
});

builder.Services.AddHttpClient<GeminiApiClient>((sp, http) =>
{
    var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<GeminiOptions>>().Value;

    http.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
    // AI Studio keys authenticate with an API key; using header keeps URLs clean.
    http.DefaultRequestHeaders.Add("x-goog-api-key", opts.ApiKey);
    http.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    http.Timeout = TimeSpan.FromSeconds(120);
});

if (builder.Environment.IsDevelopment())
{
    clientId = configuration["Authentication:Google:ClientId"];
    clientSecret = configuration["Authentication:Google:ClientSecret"];
    gameStoreConnectionString = builder.Configuration.GetConnectionString("GameStoreString");
    if (string.IsNullOrEmpty(gameStoreConnectionString))
    {
        throw new InvalidOperationException("gamestore connection string appsetting value not set.");
    }
    jwtKey = builder.Configuration["Jwt:Key"];
    if (string.IsNullOrEmpty(jwtKey))
    {
        throw new InvalidOperationException("jwtkey appsetting value not set.");
    }
}
else if (Environment.GetEnvironmentVariable("RENDER") != null) // Only on Render
{
    clientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
    clientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");
    gameStoreConnectionString = Environment.GetEnvironmentVariable("GAME_STORE_CONNECTION_STRING");
    if (string.IsNullOrEmpty(gameStoreConnectionString))
    {
        throw new InvalidOperationException("DATABASE_URL environment variable is not set.");
    }
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
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<GameStoreDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
});
builder.Services
    .AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = clientId;
        options.ClientSecret = clientSecret;
        options.CallbackPath = "/signin-google";
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey))
            };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // var accessToken =
                // context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (path.StartsWithSegments("/realtimehub"))
                {
                    var accessToken =
                        context.HttpContext.Session.GetString("JWToken");

                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        context.Token = accessToken;
                        // Console.WriteLine($"Token detected: {context.Token}");
                    }
                }

                return Task.CompletedTask;
            }
        };
    });
// builder.Services.AddAuthentication(options =>
// {
//     options.DefaultAuthenticateScheme =
//         CookieAuthenticationDefaults.AuthenticationScheme;

//     options.DefaultSignInScheme =
//         CookieAuthenticationDefaults.AuthenticationScheme;

//     options.DefaultChallengeScheme =
//         CookieAuthenticationDefaults.AuthenticationScheme;
// })
// .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
// {
//     options.LoginPath = "/Account/Login";
// })
// .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
// {
//     options.TokenValidationParameters =
//         new TokenValidationParameters
//         {
//             ValidateIssuer = false,
//             ValidateAudience = false,
//             ValidateLifetime = true,
//             ValidateIssuerSigningKey = true,
//             IssuerSigningKey =
//                 new SymmetricSecurityKey(
//                     Encoding.UTF8.GetBytes(jwtKey))
//         };

//     options.Events = new JwtBearerEvents
//     {
//         OnMessageReceived = context =>
//         {
//             var path = context.HttpContext.Request.Path;

//             if (path.StartsWithSegments("/realtimehub"))
//             {
//                 var token =
//                     context.HttpContext.Session.GetString("JWToken");

//                 if (!string.IsNullOrEmpty(token))
//                     context.Token = token;
//             }

//             return Task.CompletedTask;
//         }
//     };
// })
// .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
// {
//     options.ClientId =
//         builder.Configuration["Authentication:Google:ClientId"];

//     options.ClientSecret =
//         builder.Configuration["Authentication:Google:ClientSecret"];

//     options.CallbackPath = "/signin-google";
// });

builder.Services.AddAuthorization();

builder.Services.AddDbContext<GameStoreDbContext>(options =>
    options.UseNpgsql(gameStoreConnectionString));

builder.Services.AddDbContextFactory<GameStoreDbContext>(options =>
    options.UseNpgsql(gameStoreConnectionString),
    ServiceLifetime.Scoped);

builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ICollectionService, CollectionService>();
builder.Services.AddScoped<IGameRatingService, GameRatingService>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IAchievementService, AchievementService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<IChallengeService, ChallengeService>();
builder.Services.AddScoped<IS3Service, S3Service>();
builder.Services.AddScoped<IStreakService, StreakService>();
builder.Services.AddScoped<IUserStatsService, UserStatsService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IGameSessionService, GameSessionService>();
builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<IAiRewriteService, AiRewriteService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IRazorpayPaymentService, RazorpayPaymentService>();
builder.Services.AddScoped<IPaymentApplicationService, PaymentApplicationService>();
builder.Services.AddScoped<IPaymentRepository, EfPaymentRepository>();

builder.Services.AddHostedService<LeaderboardBackgroundService>();
builder.Services.AddHostedService<DailyChallengeGeneratorService>();

builder.Services.AddSignalR();

builder.Services.AddHttpContextAccessor(); // Needed to access session inside the provider
// builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpClient("AuthService", client =>
    {
        client.BaseAddress = new Uri("https://localhost:7168/");
    })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    });
    builder.Services.AddHttpClient("GameStoreApiService", client =>
    {
        client.BaseAddress = new Uri("https://localhost:7054/");
    })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    });
    builder.Services.AddHttpClient("PaymentApiService", client =>
    {
        client.BaseAddress = new Uri("https://localhost:7115/");
    })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    });
    builder.Services.AddHttpClient("LocalAPIClient", client =>
    {
        client.BaseAddress = new Uri("https://localhost:7051/");
    })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
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
        client.Timeout = TimeSpan.FromMinutes(10);
    });
    builder.Services.AddHttpClient("PaymentApiService", client =>
    {
        client.BaseAddress = Environment.GetEnvironmentVariable("PAYMENT_API_URL") != null
            ? new Uri(Environment.GetEnvironmentVariable("PAYMENT_API_URL"))
            : throw new Exception("Environment variable not set.");
    });
    builder.Services.AddHttpClient("LocalAPIClient", client =>
    {
        client.BaseAddress = Environment.GetEnvironmentVariable("LOCAL_API_URL") != null
            ? new Uri(Environment.GetEnvironmentVariable("LOCAL_API_URL"))
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
// app.UseMiddleware<TokenLoggingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapBlazorHub();
app.MapHub<RealtimeHub>("/realtimehub");

await app.MigrateDbAsync();

app.MapFallbackToPage("/marketplace", "/_Host");
app.MapFallbackToPage("/addGame", "/_Host");
app.MapFallbackToPage("/myCollection", "/_Host");
app.MapFallbackToPage("/mygames", "/_Host");
app.MapFallbackToPage("/addblog", "/_Host");
app.MapFallbackToPage("/blogsite", "/_Host");
app.MapFallbackToPage("/playground", "/_Host");
app.MapFallbackToPage("/analytics/trendingGames", "/_Host");
app.MapFallbackToPage("/analytics/playtime", "/_Host");
app.MapFallbackToPage("/analytics/player-growth", "/_Host");
app.MapFallbackToPage("/analytics", "/_Host");
app.MapFallbackToPage("/converter", "/_Host");
app.Run();