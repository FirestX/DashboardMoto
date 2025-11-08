using DashboardMoto;
using DashboardMoto.Entities;
using DashboardMoto.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenQA.Selenium.Chrome;

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
// WEB SCRAPING MULTISITO
// -----------------------------------------------------------

// Impostazioni Chrome (headless)
var chromeService = ChromeDriverService.CreateDefaultService();

var chromeOptions = new ChromeOptions();

var scraper = new WebScraper(chromeService, chromeOptions);
List<Motorbike> allMotorbikes = [];

// -----------------------------------------------------------
// ESECUZIONE DELLO SCRAPING MULTIPAGINA
// -----------------------------------------------------------

for (int nPag = 1; nPag <= 0; nPag++)
{
	Console.WriteLine($"\n================== PAGINA {nPag} ==================\n");

	// 🔸 Sito 1 - AutoScout24
	var autoscoutConfig = new ScrapeConfig
	{
		Url =
			$"https://www.autoscout24.it/lst-moto?sort=standard&desc=0&ustate=N%2CU&atype=B&cy=I&cat=&body=101&damaged_listing=exclude&source=detailpage_back-to-list&page={nPag}&size=40",
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

	Console.WriteLine($"🔍 AutoScout24 - Pagina {nPag}...");
	var result1 = scraper.Scrape(autoscoutConfig);
	Console.WriteLine($"✅ AutoScout24 Pagina {nPag}: trovate {result1.Count} moto.");
	allMotorbikes.AddRange(result1);

	// 🔸 Sito 2 - Mundimoto
	var site2Config = new ScrapeConfig
	{
		Url =
			$"https://mundimoto.com/it/moto-occasioni?utm_source=google&utm_medium=cpc&utm_campaign=it-mm-go-sem-generic&utm_content=147373158222&utm_term=moto+usate&gad_source=1&gad_campaignid=20267190430&gbraid=0AAAAApayHkfrEJ27z7k3htoPq-SACiz5w&gclid=EAIaIQobChMI-IO--qnYkAMVcqRQBh1k_ACUEAAYAiAAEgIYMvD_BwE&motorbike_type=&page={nPag}&size=10",
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

	Console.WriteLine($"🔍 Mundimoto - Pagina {nPag}...");
	var result2 = scraper.Scrape(site2Config);
	Console.WriteLine($"✅ Mundimoto Pagina {nPag}: trovate {result2.Count} moto.");
	allMotorbikes.AddRange(result2);

	// 🔸 Sito 3 - Moto.it
	var site3Config = new ScrapeConfig
	{
		Url = $"https://www.moto.it/moto-usate/ricerca/{nPag}?offer=&cat=sportive,super-sportive&place_rad=200",
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

	Console.WriteLine($"🔍 Moto.it - Pagina {nPag}...");
	var result3 = scraper.Scrape(site3Config);
	Console.WriteLine($"✅ Moto.it Pagina {nPag}: trovate {result3.Count} moto.");
	allMotorbikes.AddRange(result3);
}

// -----------------------------------------------------------
// RISULTATI FINALI
// -----------------------------------------------------------

Console.WriteLine();
Console.WriteLine($"Totale moto trovate: {allMotorbikes.Count}");
Console.WriteLine("-----------------------------------------");

foreach (var m in allMotorbikes)
{
	Console.WriteLine(
		$"ID: {m.Id}, Model: {m.Model}, Price: {m.Price}€, Mileage: {m.MileageKm}km, Location: {m.Location}, "
			+ $"PostDate: {m.PostDate}, HP: {m.HorsePower}, GearBox: {m.GearBox}, Brand: {m.Brand}, Fuel: {m.FuelType}"
	);
}

// -----------------------------------------------------------
// SALVATAGGIO SU DATABASE (opzionale)
// -----------------------------------------------------------

// using (var scope = app.Services.CreateScope())
// {
//     var motoRepository = scope.ServiceProvider.GetRequiredService<IMotoRepository>();
//     var motoUtilities = new MotoUtilities(motoRepository);
//     await motoUtilities.PrintInDatabase(allMotorbikes);
//     Console.WriteLine("💾 Dati salvati nel database.");
// }

// -----------------------------------------------------------
// ENDPOINT API (GET /motorbikes)
// -----------------------------------------------------------

app.MapGet(
	"/motorbikes",
	async (IMotoRepository repository) =>
	{
		return await repository.GetAll();
	}
);
app.MapPost("/motorbikes/autoscout{nPag}",
	async (int nPag) =>
	{
		for (int i = 0; i < nPag; i++)
		{
			var autoscoutConfig = new ScrapeConfig
			{
				Url =
					$"https://www.autoscout24.it/lst-moto?sort=standard&desc=0&ustate=N%2CU&atype=B&cy=I&cat=&body=101&damaged_listing=exclude&source=detailpage_back-to-list&page={i}&size=40",
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

			Console.WriteLine($"🔍 AutoScout24 - Pagina {i}...");
			var result1 = scraper.Scrape(autoscoutConfig);
			Console.WriteLine($"✅ AutoScout24 Pagina {i}: trovate {result1.Count} moto.");
		}
	}
);

app.Run();
