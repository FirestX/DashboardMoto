using DashboardMoto;
using DashboardMoto.Entities;
using DashboardMoto.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

// -----------------------------------------------------------
// CONFIGURAZIONE BASE DELL'APPLICAZIONE
// -----------------------------------------------------------

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new OpenApiInfo { Title = "Dashboard Moto API", Version = "v1" });
});

// OpenAPI e DB Context
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository moto
builder.Services.AddScoped<IMotoRepository, MotoRepository>();

// WebScraper come singleton
builder.Services.AddSingleton<WebScraper>();

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

app.MapGet(
	"/motorbikes",
	async (IMotoRepository repository) =>
	{
		return await repository.GetAll();
	}
);

app.MapPost(
	"/scrape/autoscout24",
	async (WebScraper scraper, IMotoRepository repository, int nPage = 1) =>
	{
		var allMotorbikes = new List<Motorbike>();
		
		for (int page = 1; page <= nPage; page++)
		{
			Console.WriteLine($"🔍 AutoScout24 - Pagina {page}...");
			
			var autoscoutConfig = new ScrapeConfig
			{
				Url = $"https://www.autoscout24.it/lst-moto?sort=standard&desc=0&ustate=N%2CU&atype=B&cy=I&cat=&body=101&damaged_listing=exclude&source=detailpage_back-to-list&page={page}&size=40",
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
					PostDate = "span.SellerInfo_date",
				},
			};

			var motorbikes = scraper.Scrape(autoscoutConfig);
			Console.WriteLine($"✅ AutoScout24 Pagina {page}: trovate {motorbikes.Count} moto.");
			allMotorbikes.AddRange(motorbikes);
		}

		// var motoUtilities = new MotoUtilities(repository);
		// await motoUtilities.PrintInDatabase(allMotorbikes);
		
		return Results.Ok(new { 
			source = "AutoScout24", 
			pages = nPage, 
			count = allMotorbikes.Count,
			motorbikes = allMotorbikes 
		});
	}
);

app.MapPost(
	"/scrape/mundimoto",
	async (WebScraper scraper, IMotoRepository repository, int nPage = 1) =>
	{
		var allMotorbikes = new List<Motorbike>();
		
		for (int page = 1; page <= nPage; page++)
		{
			Console.WriteLine($"🔍 Mundimoto - Pagina {page}...");
			
			var mundimotoConfig = new ScrapeConfig
			{
				Url = $"https://mundimoto.com/it/moto-occasioni?utm_source=google&utm_medium=cpc&utm_campaign=it-mm-go-sem-generic&utm_content=147373158222&utm_term=moto+usate&gad_source=1&gad_campaignid=20267190430&gbraid=0AAAAApayHkfrEJ27z7k3htoPq-SACiz5w&gclid=EAIaIQobChMI-IO--qnYkAMVcqRQBh1k_ACUEAAYAiAAEgIYMvD_BwE&motorbike_type=&page={page}&size=10",
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
					PostDate = "",
				},
			};

			var motorbikes = scraper.Scrape(mundimotoConfig);
			Console.WriteLine($"✅ Mundimoto Pagina {page}: trovate {motorbikes.Count} moto.");
			allMotorbikes.AddRange(motorbikes);
		}

		// var motoUtilities = new MotoUtilities(repository);
		// await motoUtilities.PrintInDatabase(allMotorbikes);
		
		return Results.Ok(new { 
			source = "Mundimoto", 
			pages = nPage, 
			count = allMotorbikes.Count,
			motorbikes = allMotorbikes 
		});
	}
);

app.MapPost(
	"/scrape/moto-it",
	async (WebScraper scraper, IMotoRepository repository, int nPage = 1) =>
	{
		var allMotorbikes = new List<Motorbike>();
		
		for (int page = 1; page <= nPage; page++)
		{
			Console.WriteLine($"🔍 Moto.it - Pagina {page}...");
			
			var motoItConfig = new ScrapeConfig
			{
				Url = $"https://www.moto.it/moto-usate/ricerca/{page}?offer=&cat=sportive,super-sportive&place_rad=200",
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
					PostDate = "li.app-date",
				},
			};

			var motorbikes = scraper.Scrape(motoItConfig);
			Console.WriteLine($"✅ Moto.it Pagina {page}: trovate {motorbikes.Count} moto.");
			allMotorbikes.AddRange(motorbikes);
		}

		// var motoUtilities = new MotoUtilities(repository);
		// await motoUtilities.PrintInDatabase(allMotorbikes);
		
		return Results.Ok(new { 
			source = "Moto.it", 
			pages = nPage, 
			count = allMotorbikes.Count,
			motorbikes = allMotorbikes 
		});
	}
);

app.Run();
