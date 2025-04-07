using GameStoreWeb;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// builder.Services.AddRazorPages();
// builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddServerSideBlazor();


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
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    // app.UseHttpsRedirection();
}
if (Environment.GetEnvironmentVariable("RENDER") != null) // Only on Render
{
    app.MapGet("/health", () => Results.Ok("API is running"));
}
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapFallbackToController("Blazor", "Home");
// app.MapRazorPages();

// app.MapRazorComponents<App>()
//    .AddInteractiveServerRenderMode();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=GameSummary}/{action=Index}/{id?}");

app.MapBlazorHub();
app.Run();
