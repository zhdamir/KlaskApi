using Microsoft.EntityFrameworkCore;
using KlaskApi.Models;

//for CORS
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

// Add logging configuration
/*builder.Host.ConfigureLogging(logging =>
{
    logging.AddConsole(); // Add console logging=> to be able zto see console logs (Console.WriteLine)
});*/

// Use builder.Logging instead of ConfigureLogging
builder.Logging.AddConsole();

//for CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:3000")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                      });
});

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<TurnierContext>(options => options.UseNpgsql(connectionString));



var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection(); //commented out for now=> beacuse of =>Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionMiddleware[3]
//Failed to determine the https port for redirect.=> while working on "Turnier starten"

app.UseStaticFiles();
app.UseRouting();

//for CORS
app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();
