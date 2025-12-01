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

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(config.MaxWaitSeconds));
                wait.IgnoreExceptionTypes(typeof(NoSuchElementException));

                wait.Until(d =>
                {
                    var els = d.FindElements(By.CssSelector(config.Selectors.ItemContainer));
                    return els != null && els.Count > 0;
                });

                var items = driver.FindElements(By.CssSelector(config.Selectors.ItemContainer));

                _logger.LogInformation("Trovati {ItemsCount} elementi in {Url}", items.Count, config.Url);
                foreach (var item in items)
                {
                    try
                    {
                        // Model
                        string model = TryGetText(item, config.Selectors.Model);

                        // Brand
                        string brandRaw = string.IsNullOrWhiteSpace(model) ? "" : model.Split(' ')[0];
                        BrandDto finalBrand = BrandDto.Other;
                        if (config.Url.Contains("moto.it"))
                        {
                            brandRaw = TryGetText(item, "span.app-leaf");
                        }
                        if (!string.IsNullOrEmpty(brandRaw))
                        {
                            if (Enum.TryParse<BrandDto>(brandRaw.Replace("-", "").Replace(" ", ""), true, out var parsedBrand))
                                finalBrand = parsedBrand;
                        }

                        // Price
                        double price = ParseDouble(TryGetText(item, config.Selectors.Price), removeCurrency: true);

                        // Mileage
                        double mileage = ParseDouble(TryGetText(item, config.Selectors.Mileage), removeKm: true);
                        if (config.Url.Contains("moto.it") && mileage == 0)
                        {
                            string mileageText = TryGetText(item, config.Selectors.Mileage);

                            if (!string.IsNullOrEmpty(mileageText))
                            {
                                var match = Regex.Match(mileageText, @"(?i)\bkm\s*([\d\.,]+)");

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
                        // Caso specifico: MUNDIMOTO.COM
                        else if (config.Url.Contains("mundimoto.com") && mileage == 0)
                        {
                            string mileageText = TryGetText(item, config.Selectors.Mileage);

                            if (!string.IsNullOrEmpty(mileageText))
                            {
                                var match = Regex.Match(mileageText, @"(?i)\b([\d\.,]+)\s*km\b");
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
                        // Location
                        string location = TryGetText(item, config.Selectors.Location);

                        // HorsePower
                        double horsePower = 0;
                        var hpText = TryGetText(item, config.Selectors.HorsePower);
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

                        // Gearbox
                        GearboxDto gearBox = GearboxDto.Manual;
                        var gearText = TryGetText(item, config.Selectors.GearBox).ToLowerInvariant();
                        if (gearText.Contains("automatic")) gearBox = GearboxDto.Automatic;
                        else if (gearText.Contains("semi")) gearBox = GearboxDto.SemiAutomatic;

                        // FuelType
                        FuelDto fuelType = FuelDto.Gasoline;
                        var fuelText = TryGetText(item, config.Selectors.FuelType).ToLowerInvariant();
                        if (fuelText.Contains("elettr") || fuelText.Contains("electric")) fuelType = FuelDto.Electric;
                        else if (fuelText.Contains("altro")) fuelType = FuelDto.Other;

                        // Post date: se è presente un selettore prova a parse, altrimenti DateTime.Now
                        DateTime postDate = DateTime.Now;
                        var dateText = TryGetText(item, config.Selectors.PostDate);
                        if (!string.IsNullOrWhiteSpace(dateText) && DateTime.TryParse(dateText, out var parsedDate))
                            postDate = parsedDate;
                        
                        // SellerId in base al sito
                        string sellerName = "AutoScout"; // default
                        if (config.Url.Contains("moto.it"))
                        {
                            sellerName = "Moto.it";
                        }
                        else if (config.Url.Contains("mundimoto.com"))
                        {
                            sellerName = "Mundimoto";
                        }

                        // Creazione oggetto Motorbike
                        motorbikes.Add(new MotorbikeDto{
                            HorsePower = horsePower,
                            Model = model,
                            PostDate = postDate,
                            Price = price,
                            MileageKm = mileage,
                            Location = location,
                            SellerName = sellerName,
                            BrandName = finalBrand,
                            FuelType = fuelType,
                            GearBoxType = gearBox
                            });
                    }
                    catch
                    {
                    }
                }

                // se non trova nulla salva page source per debug
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
