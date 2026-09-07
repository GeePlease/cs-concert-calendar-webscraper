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
builder.Services.AddControllers();

// Registriert Concert-Service für die Dependency Injection
builder.Services.AddScoped<ConcertService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Concert/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();


// Mapped alle Controller über ihre Attribute (z.B. [Route("api/[controller]")])
app.MapControllers();

app.Run();