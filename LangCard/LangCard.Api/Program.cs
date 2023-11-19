using LangCard.Api;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

var cloudMode = (Environment.GetEnvironmentVariable("PORT") ?? "false") != "false";
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
var url = string.Concat("http://0.0.0.0:", port);

if(cloudMode)
{
    builder.WebHost.UseUrls(url);
}

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.AllowAnyOrigin()
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

var app = builder.Build();

app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var db = new DatabaseThingy();

app.MapGet("/cards", async () =>
{
    var cards = await db.Select<CardRequest>("cards");
    return cards;
})
.WithOpenApi();

app.MapPost("/card", async ([FromBody] CardRequest model) =>
{
    model.Id = await db.Insert("cards", model);
    return model;
});

app.MapGet("/info", () => $"OK - {DateTime.UtcNow:s}");

app.Run();
