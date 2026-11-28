// EF Core: needed for UseSqlServer() and MigrateAsync(). Both are extension
// methods declared in this namespace — without the using, the compiler cannot
// see them and reports "does not contain a definition".
using Microsoft.EntityFrameworkCore;

// Where SaigonBiteDbContext and SeedData live.
using SaigonBite.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register the DbContext with dependency injection.
// WHY: ASP.NET Core constructs controllers itself and supplies whatever their
// constructors ask for. When OrderController declares
//     public OrderController(SaigonBiteDbContext db)
// the framework needs to know how to build a SaigonBiteDbContext. This line
// teaches it. Without it the app still compiles but throws at runtime:
//     "Unable to resolve service for type SaigonBiteDbContext".
// It also selects SQL Server and reads the connection string from User Secrets
// (GetConnectionString covers both appsettings.json and user secrets), so the
// connection string is never committed — as the team README promises.
// Must come BEFORE builder.Build(), which freezes the service collection.
builder.Services.AddDbContext<SaigonBiteDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Create or update the database and load demo data at startup.
// WHY A SCOPE: the DbContext is registered with a scoped lifetime — one instance
// per HTTP request. At startup there is no request and therefore no scope, so one
// is created by hand here and released when the using block ends.

// MigrateAsync() applies every pending migration, so a teammate or the instructor
// only has to clone and run; no manual ef command is needed.

// EnsureSeededAsync() loads 3 hubs, 3 brands, 12 menu items and 3 vouchers when the
// database is empty, which is what makes UC1 demonstrable.

// Must come AFTER builder.Build(), because app.Services does not exist until then.
// Note: migrating on startup suits a course project but is not production practice —
// several instances would race to apply the same migration. A real deployment runs
// migrations as a separate step.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SaigonBiteDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.EnsureSeededAsync(db);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
