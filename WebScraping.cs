using System.Globalization;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using DashboardMoto.Entities.Dtos;

namespace DashboardMoto
{
	public class SelectorSet
	{
		public string ItemContainer { get; set; } = "";
		public string Model { get; set; } = "";
		public string Price { get; set; } = "";
		public string Mileage { get; set; } = "";
		public string FuelType { get; set; } = "";
		public string GearBox { get; set; } = "";
		public string HorsePower { get; set; } = "";
		public string Location { get; set; } = "";
		public string PostDate { get; set; } = "";
	}

	public class ScrapeConfig
	{
		public string Url { get; set; } = "";
		public SelectorSet Selectors { get; set; } = new SelectorSet();
		public int MaxWaitSeconds { get; set; } = 20;
	}

	public class WebScraper
	{
		private readonly ChromeOptions _options;
		private readonly ILogger<WebScraper> _logger;
		private static readonly SemaphoreSlim _semaphore = new(3, 3);

		public WebScraper(ILogger<WebScraper> logger, ChromeOptions? options = null)
		{
			_logger = logger;

			_options = options ?? new ChromeOptions();
			_options.AddArgument("--headless=new");
			_options.AddArgument("--disable-gpu");
			_options.AddArgument("--no-sandbox");
			_options.AddArgument("--disable-dev-shm-usage");
			_options.AddArgument("--blink-settings=imagesEnabled=false");
			_options.AddArgument("--window-size=1920,1080");
			_options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
		}

		public async Task<List<MotorbikeDto>> ScrapeAsync(ScrapeConfig config)
		{
			await _semaphore.WaitAsync();
			try
			{
				_logger.LogInformation("Starting scrape for URL: {Url}", config.Url);
				return await Task.Run(() => Scrape(config));
			}
			finally
			{
				_semaphore.Release();
			}
		}

		public List<MotorbikeDto> Scrape(ScrapeConfig config)
		{
			var motorbikes = new List<MotorbikeDto>();

			try
			{
				using var service = ChromeDriverService.CreateDefaultService();
				service.HideCommandPromptWindow = true;
				service.SuppressInitialDiagnosticInformation = true;

				using var driver = new ChromeDriver(service, _options);
				driver.Navigate().GoToUrl(config.Url);

				WaitForElements(driver, config);
				var items = driver.FindElements(By.CssSelector(config.Selectors.ItemContainer));

				_logger.LogInformation("Trovati {ItemsCount} elementi in {Url}", items.Count, config.Url);
				foreach (var item in items)
				{
					try
					{
						var motorbike = ParseMotorbikeItem(item, config);
						motorbikes.Add(motorbike);
					}
					catch
					{
					}
				}

				if (motorbikes.Count == 0)
				{
					File.WriteAllText("pagesource_debug.html", driver.PageSource);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Errore nello scraping di {config.Url}: {ex.Message}");
			}

			return motorbikes;
		}

		private static void WaitForElements(ChromeDriver driver, ScrapeConfig config)
		{
			var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(config.MaxWaitSeconds));
			wait.IgnoreExceptionTypes(typeof(NoSuchElementException));
			wait.Until(d =>
			{
				var els = d.FindElements(By.CssSelector(config.Selectors.ItemContainer));
				return els != null && els.Count > 0;
			});
		}

		private static MotorbikeDto ParseMotorbikeItem(IWebElement item, ScrapeConfig config)
		{
			string model = TryGetText(item, config.Selectors.Model);
			BrandDto brand = ParseBrand(item, model, config.Url);
			double price = ParseDouble(TryGetText(item, config.Selectors.Price), removeCurrency: true);
			double mileage = ParseMileage(item, config);
			string location = TryGetText(item, config.Selectors.Location);
			double horsePower = ParseHorsePower(item, config.Selectors.HorsePower);
			GearboxDto gearBox = ParseGearbox(item, config.Selectors.GearBox);
			FuelDto fuelType = ParseFuelType(item, config.Selectors.FuelType);
			DateTime postDate = ParsePostDate(item, config.Selectors.PostDate);
			string sellerName = GetSellerName(config.Url);

			return new MotorbikeDto
			{
				HorsePower = horsePower,
				Model = model,
				PostDate = postDate,
				Price = price,
				MileageKm = mileage,
				Location = location,
				SellerName = sellerName,
				BrandName = brand,
				FuelType = fuelType,
				GearBoxType = gearBox
			};
		}

		private static BrandDto ParseBrand(IWebElement item, string model, string url)
		{
			string brandRaw = string.IsNullOrWhiteSpace(model) ? "" : model.Split(' ')[0];
			
			if (url.Contains("moto.it"))
			{
				brandRaw = TryGetText(item, "span.app-leaf");
			}

			if (!string.IsNullOrEmpty(brandRaw))
			{
				if (Enum.TryParse<BrandDto>(brandRaw.Replace("-", "").Replace(" ", ""), true, out var parsedBrand))
					return parsedBrand;
			}

			return BrandDto.Other;
		}

		private static double ParseMileage(IWebElement item, ScrapeConfig config)
		{
			double mileage = ParseDouble(TryGetText(item, config.Selectors.Mileage), removeKm: true);
			
			if (mileage == 0)
			{
				string mileageText = TryGetText(item, config.Selectors.Mileage);
				if (!string.IsNullOrEmpty(mileageText))
				{
					string pattern = config.Url.Contains("moto.it") 
						? @"(?i)\bkm\s*([\d\.,]+)" 
						: @"(?i)\b([\d\.,]+)\s*km\b";
					
					var match = Regex.Match(mileageText, pattern);
					if (match.Success)
					{
						string kmValue = match.Groups[1].Value.Trim();
						kmValue = kmValue.Replace(".", "").Replace(",", ".");
						if (double.TryParse(kmValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedMileage))
						{
							mileage = parsedMileage;
						}
					}
				}
			}

			return mileage;
		}

		private static double ParseHorsePower(IWebElement item, string selector)
		{
			double horsePower = 0;
			var hpText = TryGetText(item, selector);
			
			if (!string.IsNullOrWhiteSpace(hpText))
			{
				int start = hpText.IndexOf('(');
				int end = hpText.IndexOf("CV", StringComparison.OrdinalIgnoreCase);
				if (start != -1 && end != -1 && end > start)
				{
					var inside = hpText.Substring(start + 1, end - start - 1).Trim();
					var parts = inside.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					if (parts.Length > 0)
						double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out horsePower);
				}
			}

			return horsePower;
		}

		private static GearboxDto ParseGearbox(IWebElement item, string selector)
		{
			var gearText = TryGetText(item, selector).ToLowerInvariant();
			
			if (gearText.Contains("automatic")) return GearboxDto.Automatic;
			if (gearText.Contains("semi")) return GearboxDto.SemiAutomatic;
			
			return GearboxDto.Manual;
		}

		private static FuelDto ParseFuelType(IWebElement item, string selector)
		{
			var fuelText = TryGetText(item, selector).ToLowerInvariant();
			
			if (fuelText.Contains("elettr") || fuelText.Contains("electric")) return FuelDto.Electric;
			if (fuelText.Contains("altro")) return FuelDto.Other;
			
			return FuelDto.Gasoline;
		}

		private static DateTime ParsePostDate(IWebElement item, string selector)
		{
			var dateText = TryGetText(item, selector);
			
			if (!string.IsNullOrWhiteSpace(dateText) && DateTime.TryParse(dateText, out var parsedDate))
				return parsedDate;
			
			return DateTime.Now;
		}

		private static string GetSellerName(string url)
		{
			if (url.Contains("moto.it")) return "Moto.it";
			if (url.Contains("mundimoto.com")) return "Mundimoto";
			
			return "AutoScout";
		}

		//legge testo o "" se non presente
		private static string TryGetText(IWebElement context, string cssSelector)
		{
			if (string.IsNullOrWhiteSpace(cssSelector)) return string.Empty;
			try
			{
				var el = context.FindElement(By.CssSelector(cssSelector));
				return el?.Text?.Trim() ?? string.Empty;
			}
			catch
			{
				return string.Empty;
			}
		}

		// rimuove simboli e prova a parseare
		private static double ParseDouble(string raw, bool removeCurrency = false, bool removeKm = false)
		{
			if (string.IsNullOrWhiteSpace(raw)) return 0;

			string s = raw.Trim();
			if (removeCurrency)
			{
				s = s.Replace("€", "").Replace("EUR", "");
			}
			if (removeKm)
			{
				s = s.Replace("km", "");
			}

			// rimuovi punti come separatore migliaia e lascia la virgola o punto decimale
			s = s.Replace(".", "").Replace(",", ".");
			if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var val))
				return val;

			return 0;
		}
	}
}