using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using DashboardMoto.Entities;

namespace DashboardMoto
{
    // Config per i selettori usati dallo scraper
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

    // Config che rappresenta un sito da scansionare
    public class ScrapeConfig
    {
        public string Url { get; set; } = "";
        public SelectorSet Selectors { get; set; } = new SelectorSet();
        public int MaxWaitSeconds { get; set; } = 20;
        // eventuali altre opzioni (es. headless true/false) possono essere aggiunte qui
    }

    // Classe che esegue lo scraping a partire da ScrapeConfig
    public class WebScraper
    {
        private readonly ChromeOptions _options;
        private readonly ChromeDriverService _service;

        public WebScraper(ChromeDriverService? service = null, ChromeOptions? options = null)
        {
            _service = service ?? ChromeDriverService.CreateDefaultService();
            _service.HideCommandPromptWindow = true;
            _service.SuppressInitialDiagnosticInformation = true;

            _options = options ?? new ChromeOptions();
            // default options comode per headless scraping; il chiamante può fornire diverse options se vuole
            _options.AddArgument("--headless=new");
            _options.AddArgument("--disable-gpu");
            _options.AddArgument("--no-sandbox");
            _options.AddArgument("--disable-dev-shm-usage");
            _options.AddArgument("--blink-settings=imagesEnabled=false");
            _options.AddArgument("--window-size=1920,1080");
            _options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
        }

        // Metodo principale: riceve una configurazione e restituisce lista di Motorbike
        public List<Motorbike> Scrape(ScrapeConfig config)
        {
            var motorbikes = new List<Motorbike>();

            try
            {
                using var driver = new ChromeDriver(_service, _options);
                driver.Navigate().GoToUrl(config.Url);

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(config.MaxWaitSeconds));
                wait.IgnoreExceptionTypes(typeof(NoSuchElementException));

                // Attendi che appaia almeno un item container
                wait.Until(d =>
                {
                    var els = d.FindElements(By.CssSelector(config.Selectors.ItemContainer));
                    return els != null && els.Count > 0;
                });

                var items = driver.FindElements(By.CssSelector(config.Selectors.ItemContainer));
                int runningId = 0;

                foreach (var item in items)
                {
                    try
                    {
                        // Model
                        string model = TryGetText(item, config.Selectors.Model);

                        // Brand: estrai prima parola dal modello (puoi modificare la logica)
                        string brandRaw = string.IsNullOrWhiteSpace(model) ? "" : model.Split(' ')[0];
                        Brand finalBrand = Brand.Other;
                        if (!string.IsNullOrEmpty(brandRaw))
                        {
                            // attempt a simple map by name (estendi se serve)
                            if (Enum.TryParse<Brand>(brandRaw.Replace("-", "").Replace(" ", ""), true, out var parsedBrand))
                                finalBrand = parsedBrand;
                        }

                        // Price
                        double price = ParseDouble(TryGetText(item, config.Selectors.Price), removeCurrency: true);

                        // Mileage
                        double mileage = ParseDouble(TryGetText(item, config.Selectors.Mileage), removeKm: true);
                        if (config.Url == "https://www.moto.it/moto-usate/ricerca/1?offer=&cat=sportive,super-sportive&place_rad=200" && mileage == 0)
                        {
                            string mileageText = TryGetText(item, config.Selectors.Mileage); 
                            // esempio di input:
                            // "Rivoli (TO)\n2021\nKm 4.769\n3 ottobre 2025"

                            if (!string.IsNullOrEmpty(mileageText))
                            {
                                // Trova "Km 4.769" o simili (anche con maiuscole/minuscole o spazi variabili)
                                var match = Regex.Match(mileageText, @"(?i)\bkm\s*([\d\.,]+)");

                                if (match.Success)
                                {
                                    string kmValue = match.Groups[1].Value.Trim();

                                    // Normalizza formato numerico (rimuove separatori migliaia e uniforma decimali)
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
                            // esempio di input:
                            // "2021   614 km   A"

                            if (!string.IsNullOrEmpty(mileageText))
                            {
                                // Cerca pattern come "614 km" o "KM 5.200"
                                var match = Regex.Match(mileageText, @"(?i)\b([\d\.,]+)\s*km\b");

                                if (match.Success)
                                {
                                    string kmValue = match.Groups[1].Value.Trim();

                                    // Normalizza formato (rimuove separatori di migliaia)
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

                        // HorsePower: prova a estrarre "(NN CV)" al interno del testo
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
                        GearBox gearBox = GearBox.Manual;
                        var gearText = TryGetText(item, config.Selectors.GearBox).ToLowerInvariant();
                        if (gearText.Contains("automatic")) gearBox = GearBox.Automatic;
                        else if (gearText.Contains("semi")) gearBox = GearBox.SemiAutomatic;

                        // FuelType
                        FuelType fuelType = FuelType.Gasoline;
                        var fuelText = TryGetText(item, config.Selectors.FuelType).ToLowerInvariant();
                        if (fuelText.Contains("elettr") || fuelText.Contains("electric")) fuelType = FuelType.Electric;
                        else if (fuelText.Contains("altro")) fuelType = FuelType.Other;

                        // Post date: se è presente un selettore prova a parse, altrimenti DateTime.Now
                        DateTime postDate = DateTime.Now;
                        var dateText = TryGetText(item, config.Selectors.PostDate);
                        if (!string.IsNullOrWhiteSpace(dateText) && DateTime.TryParse(dateText, out var parsedDate))
                            postDate = parsedDate;

                        // Creazione oggetto Motorbike — adatta il costruttore se la tua entity è diversa
                        motorbikes.Add(new Motorbike(
                            0,
                            horsePower,
                            model,
                            postDate,
                            price,
                            mileage,
                            location,
                            0,              // SellerId — se lo prendi dal sito inseriscilo qui
                            finalBrand,
                            fuelType,
                            gearBox
                        ));
                    }
                    catch
                    {
                        // ignora singolo elemento e continua
                    }
                }

                // se non trovi nulla salva page source per debug (opzionale)
                if (motorbikes.Count == 0)
                {
                    System.IO.File.WriteAllText("pagesource_debug.html", driver.PageSource);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nello scraping di {config.Url}: {ex.Message}");
            }

            return motorbikes;
        }

        // helper: legge testo o "" se non presente
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

        // helper: rimuove simboli e prova a parseare
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
