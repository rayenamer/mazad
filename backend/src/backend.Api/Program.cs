using Backend.Api.Repositories;
using Backend.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<AnnonceService>();

builder.Services.AddSingleton<IAnnonceRepository>(_ =>
    InMemoryAnnonceRepository.DepuisFichier(
        Path.Combine(AppContext.BaseDirectory, "Data", "annonces.json")));

Console.WriteLine($"{builder.Services.BuildServiceProvider().GetRequiredService<IAnnonceRepository>().GetAll().Count()} annonces chargées");


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
