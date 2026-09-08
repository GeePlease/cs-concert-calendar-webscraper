// ==================================================================================
// CLASS: Program.cs
// Zentrale Einstiegsdatei, die den Start, die Konfiguration und die Abhängigkeiten 
// (Dependency Injection) einer modernen .NET-Anwendung steuert.
// ==================================================================================
using MongoDB.Driver;
using ConcertsGraz.Models;
using ConcertsGraz.Services;
using ConcertsGraz.Scrapers;   

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------------------------------------
// 1 Configuration + DB (MongoDB)
// load config settings from appsettings.json, register MongoDB-Client.
// ----------------------------------------------------------------------------------

builder.Services.Configure<ConcertsGrazDatabaseSettings>(
    builder.Configuration.GetSection("ConcertsGrazDatabase"));

// 1 Mongo Client per app only
var mongoConnection = builder.Configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017";
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnection));

// ----------------------------------------------------------------------------------
// 2 DEPENDENCY INJECTION (DI) CONTAINER - SERVICE-REGISTRIERUNG
// ----------------------------------------------------------------------------------

// Controllers: Registriert alle Controller (z.B. ScraperController, ConcertController),
// damit sie auf HTTP-Anfragen reagieren können.
builder.Services.AddControllers();

// SWAGGER SERVICEREGISTRIERUNG (Hier hinzufügen):
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database Services
builder.Services.AddScoped<ConcertService>();

// Konkrete Scraper-Registrierung
builder.Services.AddScoped<ClubWakuumScraper>();   
builder.Services.AddScoped<PpcScraper>(); 
builder.Services.AddScoped<CafeWolfScraper>();

// Haupt-ScraperService
builder.Services.AddScoped<ScraperService>();

// ----------------------------------------------------------------------------------
// SCRAPER-REGISTRIERUNG :
// Wir registrieren jede Scraper-Klasse direkt. Dadurch weiß der DI-Container,
// wie er diese beim Erstellen des 'ScraperService' erzeugen und übergeben muss
// ----------------------------------------------------------------------------------
builder.Services.AddScoped<ClubWakuumScraper>();   
builder.Services.AddScoped<PpcScraper>(); 
builder.Services.AddScoped<CafeWolfScraper>();

// Der Haupt-ScraperService (verlangt im Konstruktor genau die 3 Scraper oben)
builder.Services.AddScoped<ScraperService>();

// ----------------------------------------------------------------------------------
// 3. PIPELINE SETUP & START
// Baut die Anwendung mit allen registrierten Services zusammen und definiert,
// wie eingehende HTTP-Requests verarbeitet werden (Middleware Pipeline).
// ----------------------------------------------------------------------------------

var app = builder.Build();

// SWAGGER PIPELINE ACTIVATION :
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); 
}
else
{
    app.UseExceptionHandler("/Concert/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();

// Aktiviert das Mapping: Verknüpft eingehende URLs (z.B. POST /api/scraper/run) 
// mit den passenden Controller-Methoden ([HttpPost("run")]).
app.MapControllers();

// Startet die App und lauscht auf eingehende Requests.
app.Run();