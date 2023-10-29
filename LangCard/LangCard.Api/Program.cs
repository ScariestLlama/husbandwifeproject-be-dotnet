using Google.Cloud.Datastore.V1;
using Grpc.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using static Grpc.Core.Metadata;

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

var envVars = Environment.GetEnvironmentVariables();

var host = envVars["DATASTOREDB_HOST"].ToString();
var project = envVars["DATASTOREDB_PROJECT"].ToString();
var port = Convert.ToInt32(envVars["DATASTOREDB_PORT"]);

var dsBuilder = new DatastoreClientBuilder();
dsBuilder.ChannelCredentials = ChannelCredentials.Insecure;
dsBuilder.Endpoint = $"{host}:{port}";
var client = dsBuilder.Build();

var db = DatastoreDb.Create(project, "", client);

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/cards", async () =>
{
    var cards = new List<CardRequest>();
    var results = await db.RunQueryAsync(new Query("cards"));
    foreach (var result in results.Entities)
    {

        if (result == null)
        {
            continue;
        }

        var card = new CardRequest();
        card.Id = result.Key.Path.Last().Name;
        foreach (var prop in result.Properties)
        {
            if(prop.Key.Equals("welsh", StringComparison.OrdinalIgnoreCase))
            {
                card.Welsh = prop.Value.StringValue;

            }

            if(prop.Key.Equals("english", StringComparison.OrdinalIgnoreCase))
            {
                card.English = prop.Value.StringValue;
            }
        }
        cards.Add(card);
    }

    return cards;
})
.WithOpenApi();

app.MapPost("/card", async ([FromBody] CardRequest model) =>
{
    var entity = new Entity()
    {
        Key = db.CreateKeyFactory("cards").CreateKey(Guid.NewGuid().ToString())
    };

    entity["welsh"] = model.Welsh;
    entity["english"] = model.English;

    var key = await db.InsertAsync(entity);

    model.Id = entity.Key.Path.Last().Name;

    return model;
});

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
