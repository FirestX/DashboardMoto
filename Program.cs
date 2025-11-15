using DashboardMoto;
using DashboardMoto.Entities;
using DashboardMoto.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenQA.Selenium.Chrome;
using Serilog;

// -----------------------------------------------------------
// CONFIGURAZIONE BASE DELL'APPLICAZIONE
// -----------------------------------------------------------

var builder = WebApplication.CreateBuilder(args);
// Serilog
builder.Host.UseSerilog((context, configuration) =>
        {
            configuration.ReadFrom.Configuration(context.Configuration);
        });

// Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Dashboard Moto API",
        Version = "v1"
    });
});

// OpenAPI e DB Context
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository moto
builder.Services.AddScoped<IMotoRepository, MotoRepository>();

// WebScraper service
builder.Services.AddScoped<WebScraper>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dashboard Moto API V1");
});

// -----------------------------------------------------------
// ENDPOINT API
// -----------------------------------------------------------

app.MapGet("/motorbikes", async (IMotoRepository repository) =>
{
    return await repository.GetAll();
});

app.MapPost("/motorbikes/autoscout24/{nPage:int}", async (int nPage, IMotoRepository repository, WebScraper scraper) =>
{
    var allMotorbikes = new List<Motorbike>();

    for (int i = 1; i <= nPage; i++)
    {
        var config = new ScrapeConfig
        {
            Url = $"https://www.autoscout24.it/lst-moto?sort=standard&desc=0&ustate=N%2CU&atype=B&cy=I&cat=&body=101&damaged_listing=exclude&source=detailpage_back-to-list&page={i}&size=40",
            Selectors = new SelectorSet
            {
                ItemContainer = "div.ListItem_wrapper__TxHWu",
                Model = "span.ListItem_title_bold__iQJRq",
                Price = "p.Price_price__APlgs",
                Mileage = "span[data-testid='VehicleDetails-mileage_road']",
                FuelType = "span[data-testid='VehicleDetails-gas_pump']",
                GearBox = "span[data-testid='VehicleDetails-gearbox']",
                HorsePower = "span[data-testid='VehicleDetails-speedometer']",
                Location = "span[data-testid='sellerinfo-address']",
                PostDate = "span.SellerInfo_date"
            }
        };

        Console.WriteLine($"AutoScout24 - Pagina {i}/{nPage}...");
        var result = scraper.Scrape(config);
        Console.WriteLine($"AutoScout24 Pagina {i}: trovate {result.Count} moto.");
        allMotorbikes.AddRange(result);
    }

    var motoUtilities = new MotoUtilities(repository);
    // await motoUtilities.PrintInDatabase(allMotorbikes);
    Console.WriteLine($"Totale moto trovate: {allMotorbikes.Count}");

    return Results.Ok(new { totalMotorbikes = allMotorbikes.Count, motorbikes = allMotorbikes });
});

app.MapPost("/motorbikes/mundimoto/{nPage:int}", async (int nPage, IMotoRepository repository, WebScraper scraper) =>
{
    var allMotorbikes = new List<Motorbike>();

    for (int i = 1; i <= nPage; i++)
    {
        var config = new ScrapeConfig
        {
            Url = $"https://mundimoto.com/it/moto-occasioni?utm_source=google&utm_medium=cpc&utm_campaign=it-mm-go-sem-generic&utm_content=147373158222&utm_term=moto+usate&gad_source=1&gad_campaignid=20267190430&gbraid=0AAAAApayHkfrEJ27z7k3htoPq-SACiz5w&gclid=EAIaIQobChMI-IO--qnYkAMVcqRQBh1k_ACUEAAYAiAAEgIYMvD_BwE&motorbike_type=&page={i}&size=10",
            Selectors = new SelectorSet
            {
                ItemContainer = "div.group.relative.flex.h-full.w-full.flex-col",
                Model = "h3.text-base",
                Price = "h3.font-semibold.m-0.text-3xl",
                Mileage = "div.flex.w-full.flex-wrap.gap-x-4 p",
                FuelType = "",
                GearBox = "",
                HorsePower = "",
                Location = "span[class*='location']",
                PostDate = ""
            }
        };

        Console.WriteLine($"Mundimoto - Pagina {i}/{nPage}...");
        var result = scraper.Scrape(config);
        Console.WriteLine($"Mundimoto Pagina {i}: trovate {result.Count} moto.");
        allMotorbikes.AddRange(result);
    }

    var motoUtilities = new MotoUtilities(repository);
    // await motoUtilities.PrintInDatabase(allMotorbikes);
    Console.WriteLine($"Totale moto trovate: {allMotorbikes.Count}");

    return Results.Ok(new { totalMotorbikes = allMotorbikes.Count, motorbikes = allMotorbikes });
});

app.MapPost("/motorbikes/motoit/{nPage:int}", async (int nPage, IMotoRepository repository, WebScraper scraper) =>
{
    var allMotorbikes = new List<Motorbike>();

    for (int i = 1; i <= nPage; i++)
    {
        var config = new ScrapeConfig
        {
            Url = $"https://www.moto.it/moto-usate/ricerca/{i}?offer=&cat=sportive,super-sportive&place_rad=200",
            Selectors = new SelectorSet
            {
                ItemContainer = "div.app-ad-list-item",
                Model = "h2.app-titles",
                Price = "div.app-price",
                Mileage = "ul.app-specs",
                FuelType = "",
                GearBox = "",
                HorsePower = "",
                Location = "ul.app-specs",
                PostDate = "li.app-date"
            }
        };

        Console.WriteLine($"Moto.it - Pagina {i}/{nPage}...");
        var result = scraper.Scrape(config);
        Console.WriteLine($"Moto.it Pagina {i}: trovate {result.Count} moto.");
        allMotorbikes.AddRange(result);
    }

    var motoUtilities = new MotoUtilities(repository);
    // await motoUtilities.PrintInDatabase(allMotorbikes);
    Console.WriteLine($"Totale moto trovate: {allMotorbikes.Count}");

    return Results.Ok(new { totalMotorbikes = allMotorbikes.Count, motorbikes = allMotorbikes });
});

app.Run();
