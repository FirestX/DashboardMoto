using DashboardMoto;
using DashboardMoto.Entities;
using DashboardMoto.Entities.Dtos;
using DashboardMoto.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
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
builder.Services.AddScoped<MotoSeeder>();

var app = builder.Build();

var seeder = app.Services.CreateScope().ServiceProvider.GetRequiredService<MotoSeeder>();
await seeder.SeedAsync();

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

app.MapPost("/motorbikes/autoscout24/{nPage:int}", async (int nPage, IMotoRepository repository, AppDbContext dbContext, WebScraper scraper) =>
{
    var scrapeTasks = new List<Task<List<MotorbikeDto>>>();

    for (int i = 1; i <= nPage; i++)
    {
        int pageNum = i; // Capture loop variable
        var config = new ScrapeConfig
        {
            Url = $"https://www.autoscout24.it/lst-moto?sort=standard&desc=0&ustate=N%2CU&atype=B&cy=I&cat=&body=101&damaged_listing=exclude&source=detailpage_back-to-list&page={pageNum}&size=40",
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

        scrapeTasks.Add(scraper.ScrapeAsync(config));
    }

    Console.WriteLine($"AutoScout24 - Scraping {nPage} pages in parallel (max 3 concurrent)...");
    var results = await Task.WhenAll(scrapeTasks);
    var allMotorbikes = results.SelectMany(list => list).ToList();
    
    Console.WriteLine($"AutoScout24 - Total found: {allMotorbikes.Count} motorbikes");

    var motoUtilities = new MotoUtilities(repository, dbContext);
    await motoUtilities.PrintInDatabase(allMotorbikes);

    return Results.Ok(new { totalMotorbikes = allMotorbikes.Count, pagesScraped = nPage, motorbikes = allMotorbikes });
});

app.MapPost("/motorbikes/mundimoto/{nPage:int}", async (int nPage, IMotoRepository repository, AppDbContext dbContext, WebScraper scraper) =>
{
    var scrapeTasks = new List<Task<List<MotorbikeDto>>>();

    for (int i = 1; i <= nPage; i++)
    {
        int pageNum = i; // Capture loop variable
        var config = new ScrapeConfig
        {
            Url = $"https://mundimoto.com/it/moto-occasioni?utm_source=google&utm_medium=cpc&utm_campaign=it-mm-go-sem-generic&utm_content=147373158222&utm_term=moto+usate&gad_source=1&gad_campaignid=20267190430&gbraid=0AAAAApayHkfrEJ27z7k3htoPq-SACiz5w&gclid=EAIaIQobChMI-IO--qnYkAMVcqRQBh1k_ACUEAAYAiAAEgIYMvD_BwE&motorbike_type=&page={pageNum}&size=10",
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

        scrapeTasks.Add(scraper.ScrapeAsync(config));
    }

    Console.WriteLine($"Mundimoto - Scraping {nPage} pages in parallel (max 3 concurrent)...");
    var results = await Task.WhenAll(scrapeTasks);
    var allMotorbikes = results.SelectMany(list => list).ToList();
    
    Console.WriteLine($"Mundimoto - Total found: {allMotorbikes.Count} motorbikes");

    var motoUtilities = new MotoUtilities(repository, dbContext);
    // await motoUtilities.PrintInDatabase(allMotorbikes);

    return Results.Ok(new { totalMotorbikes = allMotorbikes.Count, pagesScraped = nPage, motorbikes = allMotorbikes });
});

app.MapPost("/motorbikes/motoit/{nPage:int}", async (int nPage, IMotoRepository repository, AppDbContext dbContext, WebScraper scraper) =>
{
    var scrapeTasks = new List<Task<List<MotorbikeDto>>>();

    for (int i = 1; i <= nPage; i++)
    {
        int pageNum = i; // Capture loop variable
        var config = new ScrapeConfig
        {
            Url = $"https://www.moto.it/moto-usate/ricerca/{pageNum}?offer=&cat=sportive,super-sportive&place_rad=200",
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

        scrapeTasks.Add(scraper.ScrapeAsync(config));
    }

    Console.WriteLine($"Moto.it - Scraping {nPage} pages in parallel (max 3 concurrent)...");
    var results = await Task.WhenAll(scrapeTasks);
    var allMotorbikes = results.SelectMany(list => list).ToList();
    
    Console.WriteLine($"Moto.it - Total found: {allMotorbikes.Count} motorbikes");

    var motoUtilities = new MotoUtilities(repository, dbContext);
    // await motoUtilities.PrintInDatabase(allMotorbikes);

    return Results.Ok(new { totalMotorbikes = allMotorbikes.Count, pagesScraped = nPage, motorbikes = allMotorbikes });
});

app.Run();
