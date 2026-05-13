using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TunisiaStay.Data;
using TunisiaStay.Models;
using TunisiaStay.Services;

var builder = WebApplication.CreateBuilder(args);
// Forcer la persistance des clés DataProtection (résout les problèmes d'antiforgery token)
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "Keys")))
    .SetApplicationName("TunisiaStay");

// ── Database ──────────────────────────────────────────────────────────
// SQLite par défaut. Pour SQL Server : remplacer UseSqlite par UseSqlServer
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (connectionString != null && connectionString.StartsWith("postgresql://"))
{
    // Convertir URL Railway vers format Npgsql
    var uri = new Uri(connectionString);
    var userInfo = uri.UserInfo.Split(':');
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
    builder.Services.AddDbContext<ApplicationDbContext>(opts =>
        opts.UseNpgsql(connectionString));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(opts =>
        opts.UseSqlite(connectionString));
}

// ── ASP.NET Core Identity ─────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opts =>
{
    opts.Password.RequiredLength = 6;
    opts.Password.RequireNonAlphanumeric = false;
    opts.Password.RequireUppercase = false;
    opts.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ── Redirection Login/AccessDenied ────────────────────────────────────
builder.Services.ConfigureApplicationCookie(opts =>
{
    opts.LoginPath = "/Account/Login";
    opts.AccessDeniedPath = "/Account/AccessDenied";
});

// ── Authorization policies ────────────────────────────────────────────
builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    opts.AddPolicy("UserOrAdmin", p => p.RequireRole("User", "Admin"));
    opts.AddPolicy("HotelierOnly", p => p.RequireRole("Hotelier"));
});

// ── Services applicatifs (Repository Pattern + Unit of Work) ──────────
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<IChambreRepository, ChambreRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IImageService, ImageService>();

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddRazorPages();

var app = builder.Build();

// ── Pipeline ──────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Area route (Admin)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ── Auto-migrate + Seed au premier démarrage ──────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db      = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    db.Database.Migrate();

    foreach (var role in new[] { "Admin", "User", "Hotelier" })
        if (!await roleMgr.RoleExistsAsync(role))
            await roleMgr.CreateAsync(new IdentityRole(role));

    // Compte admin par défaut
    const string adminEmail = "admin@tunisiastay.tn";
    if (await userMgr.FindByEmailAsync(adminEmail) is null)
    {
        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email    = adminEmail,
            FullName = "Administrateur TunisiaStay",
            EmailConfirmed = true
        };
        var result = await userMgr.CreateAsync(admin, "Admin@123");
        if (result.Succeeded)
            await userMgr.AddToRoleAsync(admin, "Admin");
    }

}
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");
