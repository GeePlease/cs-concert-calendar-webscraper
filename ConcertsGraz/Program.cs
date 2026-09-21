
using MongoDB.Driver;
using ConcertsGraz.Data;
using ConcertsGraz.Services;
using ConcertsGraz.Scrapers;  
using ConcertsGraz.Utilities;
using Microsoft.Extensions.Options;
using ConcertsGraz.ErrorHandling;
using ConcertsGraz.Interfaces;

// ==================================================================================
// CLASS: Program.cs
// ==================================================================================

// from Asp.Net.Core
var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------------------------------------
// 1. CONFIGURATION + DB (MongoDB)
// ----------------------------------------------------------------------------------
builder.Services.Configure<ConcertsGrazDatabaseSettings>(
    builder.Configuration.GetSection("ConcertsGrazDatabase"));

var mongoConnection = builder.Configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017";
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnection));

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<ConcertsGrazDatabaseSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(settings.DatabaseName);
});

// ----------------------------------------------------------------------------------
// 2. DEPENDENCY INJECTION (DI) CONTAINER
// ----------------------------------------------------------------------------------
builder.Services.AddControllers();

// Global Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// HTTPS-Port explizit festlegen (löst die Redirection-Warnung)
builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 7170; // anpassen wennn notwendig
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database Services & Utilities
builder.Services.AddScoped<ConcertService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TestDataSeeder>();

// Scraper-Registrierung als Interface
builder.Services.AddScoped<IScraper, ClubWakuumScraper>();
builder.Services.AddScoped<IScraper, PpcScraper>();
builder.Services.AddScoped<IScraper, CafeWolfScraper>();
builder.Services.AddScoped<ScraperService>();

// CORS Registrierung
builder.Services.AddCors(o => o.AddDefaultPolicy(p => 
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// Session Configuration
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);  // Session-Dauer
    options.Cookie.HttpOnly = true;               // Schutz gegen XSS
    options.Cookie.IsEssential = true;            // DSGVO-Notwendigkeit
});

// ----------------------------------------------------------------------------------
// 3. PIPELINE SETUP & START
// ----------------------------------------------------------------------------------
var app = builder.Build();

// TestDataSeeder & Scraper - run at start
using (var scope = app.Services.CreateScope())
{
    // seed test data
    var seeder = scope.ServiceProvider.GetRequiredService<TestDataSeeder>();
    await seeder.SeedAllAsync();
    
    // run scrapers
    var scraperService = scope.ServiceProvider.GetRequiredService<ScraperService>();
    var concertService = scope.ServiceProvider.GetRequiredService<ConcertService>();
    
    var scrapedConcerts = await scraperService.RunAllAsync();
    await concertService.SaveScrapedConcertsAsync(scrapedConcerts);
    
}

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); 
}
else
{
    app.UseHsts();
}

// global exception handler
app.UseExceptionHandler();

app.UseHttpsRedirection();

// Statische Dateien aus wwwroot bereitstellen (index.html, style.css, app.js)
// WICHTIG: UseDefaultFiles MUSS vor UseStaticFiles stehen!
app.UseDefaultFiles(); 
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseCors();
app.UseAuthorization();

// REST-API Controller Endpunkte mappen
app.MapControllers();

// run APP
app.Run();