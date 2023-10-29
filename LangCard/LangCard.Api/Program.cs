using LangCard.Api;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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

app.Run();
