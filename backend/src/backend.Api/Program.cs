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

// l'import marche , on enléve cette ligne :D

var app = builder.Build();


app.UseMiddleware<Backend.Api.Infrastructure.ExceptionHandlingMiddleware>();

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
public partial class Program { }