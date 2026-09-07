using MongoDB.Driver;
using ConcertsGraz.Models;
using ConcertsGraz.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<ConcertsGrazDatabaseSettings>(
    builder.Configuration.GetSection("ConcertsGrazDatabase"));

// ----- MongoDB  -----
var mongoConnection = builder.Configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017";
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnection));
// -----------------------------


// Add services to the container.
builder.Services.AddControllersWithViews();

// Registriert deinen Concert-Service für die Dependency Injection
builder.Services.AddScoped<ConcertService>();

var app = builder.Build();

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