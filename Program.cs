using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Hubs;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// === LOKALIZACIJA: Registracija lokalizacijskih servisa ===
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();
// === KRAJ lokalizacijskih servisa ===

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password = new PasswordOptions
    {
        RequireDigit = false,
        RequireLowercase = false,
        RequireNonAlphanumeric = false,
        RequireUppercase = false,
        RequiredLength = 3
    };
    options.User.RequireUniqueEmail = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Eksplicitna registracija IWebHostEnvironment (opciono, ali korisno za jasnoću)
builder.Services.AddSingleton<IWebHostEnvironment>(sp => builder.Environment);

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100MB
});

builder.Services.AddSignalR();
builder.Services.AddControllers();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "StudentHub API v1");
    options.RoutePrefix = "swagger";
});

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles(); // Omogućava posluživanje fajlova iz wwwroot

// === LOKALIZACIJA: Middleware za culture switching ===
var supportedCultures = new[] { new CultureInfo("bs"), new CultureInfo("en") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("bs"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});
// === KRAJ lokalizacija middleware ===

app.UseRouting();

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/index.html")
    {
        context.Response.Redirect("/Home/Index");
        return;
    }
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.MapHub<NotificationHub>("/notificationHub");

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/Home/Index");
        return;
    }
    await next();
});

app.Run();
