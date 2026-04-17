using Gurbani.Data;
using Gurbani.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register MVC (controllers + views)
builder.Services.AddControllersWithViews();

// Register GurbaniService with an HttpClient
// AddHttpClient handles creating and reusing HttpClient safely
builder.Services.AddHttpClient<GurbaniService>();

// Register the database context using the connection string from appsettings.json
// This tells the app to use SQL Server as the database
builder.Services.AddDbContext<GurbaniDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Run any pending EF migrations on startup — creates tables on first deploy to Azure
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GurbaniDbContext>();
    db.Database.Migrate();
}

// In production, send users to the error page if something crashes
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // Enforces HTTPS in browsers for security
}

app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS
app.UseRouting();          // Enable routing so URLs map to controllers
app.UseAuthorization();    // Enable authorization middleware (needed even if not using auth yet)
app.MapStaticAssets();     // Serve files from wwwroot (CSS, JS, images)

// Default route: maps URLs like /Controller/Action/id
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
