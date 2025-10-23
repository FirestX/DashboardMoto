using DashboardMoto.Entities;
using DashboardMoto.Persistence;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSwaggerGen(options =>
    {
      options.SwaggerDoc("v1", new OpenApiInfo
      {
        Title = "Dashboard Moto API",
        Version = "v1"
      });
    });

builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IMotoRepository, MotoRepository>();

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
}
foreach (var motorbike in motorbikes)
{
}

app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dashboard Moto API V1");
});
app.MapGet("/motorbikes", async (IMotoRepository repository) =>
{
    return await repository.GetAll();
});
app.Run();

