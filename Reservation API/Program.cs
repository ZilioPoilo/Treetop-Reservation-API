using Microsoft.OpenApi.Models;
using Reservation_API.AppMapping;
using Reservation_API.Services;
using Reservation_API.Services.DataServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// AutoMappper
builder.Services.AddAutoMapper(typeof(AppMappingService));

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo()
    {
        Title = "Reservation API",
        Description = "Reservation API (ASP.NET Core 6.0)",
        TermsOfService = new Uri("https://github.com/openapitools/openapi-generator"),
        Contact = new OpenApiContact
        {
            Name = "Yurii",
            Email = "frolyakyurii@gmail.com"
        },
        Version = "v1",
    });
});

builder.Services.AddSingleton<MongoDbConnectionService>();
builder.Services.AddSingleton<ReservationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
