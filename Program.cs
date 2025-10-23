using DashboardMoto.Entities;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// ----------------- START: Selenium scraping -----------------
var url = "https://www.autoscout24.it/lst-moto?sort=standard&desc=0&ustate=N%2CU&atype=B&cy=I&cat=&body=101&damaged_listing=exclude&source=homepage_search-mask";
string paginaCompleta = "div.ListItem_wrapper__TxHWu";
string linkModello = "span.ListItem_title_bold__iQJRq";
string linkPrezzo = "p.Price_price__APlgs";
string linkKm = "span[data-testid='VehicleDetails-mileage_road']";
string linkFuelType = "span[data-testid='VehicleDetails-gas_pump']";
string linkGearBox = "span[data-testid='VehicleDetails-gearbox']";
string linkHorsePower = "span[data-testid='VehicleDetails-speedometer']";
string linkLocation = "span[class^='SellerInfo_address__leRMu']";
string linkData = "span.SellerInfo_date";

// Configuro il servizio per nascondere la finestra console del chromedriver
var service = ChromeDriverService.CreateDefaultService();
service.HideCommandPromptWindow = true;
service.SuppressInitialDiagnosticInformation = true;

// Opzioni: headless, user agent (importantissimo), disable gpu, no-sandbox
var options = new ChromeOptions();
options.AddArgument("--headless=new"); // usa "--headless=new" per versioni recenti di Chrome
options.AddArgument("--disable-gpu");
options.AddArgument("--no-sandbox");
options.AddArgument("--disable-dev-shm-usage");
options.AddArgument("--blink-settings=imagesEnabled=false"); // opzionale: non caricare immagini
options.AddArgument("--window-size=1920,1080");
// Imposta user-agent per sembrare un browser reale
options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");

// Opzione: se vuoi vedere cosa succede, commenta la riga headless sopra

List<Motorbike> motorbikes = new();

try
{
    using var driver = new ChromeDriver(service, options);

    // Naviga
    driver.Navigate().GoToUrl(url);

    // Aspetta che compaiano gli elementi (max 20s)
    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
    wait.IgnoreExceptionTypes(typeof(NoSuchElementException));

    // Aspetta che ci sia almeno 1 elemento con il selettore
    wait.Until(d =>
    {
        var els = d.FindElements(By.CssSelector(paginaCompleta));
        return els != null && els.Count > 0;
    });

    // Ora prendi tutti i blocchi
    var items = driver.FindElements(By.CssSelector(paginaCompleta));
    int id = 0;

    foreach (var item in items)
{
    try
    {
        // Model / Titolo
        var modelElem = item.FindElement(By.CssSelector(linkModello));
        string model = modelElem.Text?.Trim() ?? "";

        // Prezzo
        double price = 0;
        try
        {
            var priceElem = item.FindElement(By.CssSelector(linkPrezzo));
            var priceText = priceElem.Text.Replace("€", "").Replace(".", "").Replace(",", "").Replace("-", "").Trim();
            double.TryParse(priceText, out price);
        }
        catch { }

        // MileageKm
        double mileage = 0;
        try
        {
            var kmElem = item.FindElement(By.CssSelector(linkKm));
            var kmText = kmElem.Text.Replace("km", "").Replace(".", "").Trim();
            double.TryParse(kmText, out mileage);
        }
        catch { }

        // Location
        string location = "";
        try
        {
            var locElem = item.FindElement(By.CssSelector(linkLocation));
            location = locElem.Text.Trim();
        }
        catch { }
        
        // PostDate: non disponibile, uso data corrente
        DateTime? date = null;
        try
        {
            var dateElem = item.FindElement(By.CssSelector(linkData));
            date = DateTime.Parse(dateElem.Text.Trim());
        }
        catch { }

        // URL → Id generico
        //string link = "";
        id++;
        /*try
        {
            var linkElem = item.FindElement(By.CssSelector("a.ListItem_title_new_design__QIU2b"));
            link = linkElem.GetAttribute("href");
            // prova a estrarre ID numerico da URL
            var match = System.Text.RegularExpressions.Regex.Match(link, @"AS(\d+)");
            if (match.Success) id = int.Parse(match.Groups[1].Value);
        }
        catch { }*/
        
        // FuelType 
        string fuelType = null;
        try
        {
            var fuelElem = item.FindElement(By.CssSelector(linkFuelType));
            fuelType = fuelElem.Text.Trim();
        }
        catch { }
        
        // GearBox
        string gearBox = null;
        try
        {
            var gearElem = item.FindElement(By.CssSelector(linkGearBox));
            gearBox = gearElem.Text.Trim();
        }
        catch { }
        
        //HP
        double horsePower = 0;
        try
        {
            var hpElem = item.FindElement(By.CssSelector(linkHorsePower));
            var hpText = hpElem.Text.Replace("CV", "").Trim();
            double.TryParse(hpText, out horsePower);
        }
        catch {
        }
        
        Console.WriteLine($"Trovata moto: ID={id}, Model={model}, Price={price}€, Mileage={mileage}km, Location={location}, HorsePower={horsePower}CV, FuelType={fuelType}, GearBox={gearBox}, Posted on: {date}");

        // A questo punto possiamo costruire l'oggetto Motorbike
        motorbikes.Add(new Motorbike(
            id,
            horsePower,               // HorsePower non disponibile
            model,
            DateTime.Now,    // PostDate
            price,
            null,            // GearBox
            mileage,
            location,
            null,            // Brand
            null,            // FuelType
            0                // SellerId
        ));
    }
    catch
    {
        // Salta eventuali blocchi vuoti
    }
}

    // Debug: se non trovi elementi, salva page source su file per ispezionare
    if (motorbikes.Count == 0)
    {
        System.IO.File.WriteAllText("pagesource.html", driver.PageSource);
        Console.WriteLine("Nessun elemento trovato: ho salvato pagesource.html nella cartella del progetto.");
    }
}
catch (Exception ex)
{
    Console.WriteLine("Errore durante lo scraping: " + ex.Message);
}
// ----------------- END: Selenium scraping -----------------
Console.WriteLine();
Console.WriteLine();
Console.WriteLine();
// Stampa i risultati trovati
foreach (var m in motorbikes)
{
    Console.WriteLine($"ID: {m.Id}, Model: {m.Model}, Price: {m.Price}€, Mileage: {m.MileageKm}km, Location: {m.Location}, Posted on: {m.PostDate}, HorsePower: {m.HorsePower}, GearBox: {m.GearBox}, Brand: {m.Brand}, FuelType: {m.FuelType}, SellerId: {m.SellerId}");
}

app.UseHttpsRedirection();

app.Run();
