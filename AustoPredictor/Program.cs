using System.Security.Claims;
using AustoPredictor.Components;
using AustoPredictor.Data;
using AustoPredictor.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found. Set it in appsettings.json / appsettings.Development.json or via configuration.");

builder.Services.AddDbContextFactory<AustoPredictorDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<PredictionService>();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/admin/login";
        options.AccessDeniedPath = "/admin/login";
        options.ExpireTimeSpan = TimeSpan.FromHours(12);
        options.SlidingExpiration = true;
        options.Cookie.Name = "AustoPredictor.Admin";
    });

var app = builder.Build();

// Apply any pending EF Core migrations on startup, and make sure the app fails fast
// with a clear message if the database is not reachable rather than surfacing odd
// errors on the first page load.
using (var scope = app.Services.CreateScope())
{
    var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AustoPredictorDbContext>>();
    await using var dbContext = await dbContextFactory.CreateDbContextAsync();
    await dbContext.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Admin login/logout are handled by a plain HTTP POST (not a Blazor form) so that
// signing the cookie in works reliably regardless of render mode. These live under
// /api/... (rather than /admin/login) so they don't collide with the Razor
// component routed at /admin/login, which Blazor also maps a POST handler for.
app.MapPost("/api/admin/login", async (HttpContext http, IConfiguration config) =>
{
    var form = await http.Request.ReadFormAsync();
    var password = form["password"].ToString();
    var returnUrl = form["returnUrl"].ToString();
    if (string.IsNullOrEmpty(returnUrl) || !Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
    {
        returnUrl = "/admin";
    }

    var storedHash = config["AdminAuth:PasswordHash"];
    if (!AdminPasswordHasher.Verify(password, storedHash))
    {
        return Results.Redirect($"/admin/login?returnUrl={Uri.EscapeDataString(returnUrl)}&error=1");
    }

    var claims = new List<Claim> { new(ClaimTypes.Name, "Admin") };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

    return Results.Redirect(returnUrl);
});

app.MapPost("/api/admin/logout", async (HttpContext http) =>
{
    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
});

app.Run();
