using DashboardMoto.Entities;
using HtmlAgilityPack;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
HtmlWeb web = new();
var document= web.Load("https://www.autoscout24.it/lst-moto?sort=standard&desc=0&ustate=N%2CU&atype=B&cy=I&cat=&body=101&damaged_listing=exclude&source=homepage_search-mask");
List<Motorbike> motorbikes = [];
var motrobikeBlockElement = document.DocumentNode.QuerySelectorAll("div.ListItem_wrapper__TxHWu");
foreach (var element in motrobikeBlockElement)
{
  var name = element.QuerySelector("span.ListItem_title_bold__iQJRq").InnerText;
  var motorbike = new Motorbike(name);
  motorbikes.Add(motorbike);
}
foreach (var motorbike in motorbikes)
{
  Console.WriteLine(motorbike.Name);
}

app.UseHttpsRedirection();

app.Run();

