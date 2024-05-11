using Microsoft.EntityFrameworkCore;
using TareasWebApi.Context;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//registrar servicio para  conexion BD
var connectionString = builder.Configuration.GetConnectionString("Connection");
builder.Services.AddDbContext<AppDbContext>(
    options => options.UseSqlServer(connectionString)
);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Creando servico cache
builder.Services.AddOutputCache(opciones =>
{
    opciones.AddPolicy("DesactivarCache", builder => builder.Expire(TimeSpan.FromSeconds(30)).Tag("Cache"));
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseOutputCache(); 
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
