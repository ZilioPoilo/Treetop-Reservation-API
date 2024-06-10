using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Reservation_API.AppMapping;
using Reservation_API.Services;
using Reservation_API.Services.DataServices;
using Serilog;
using Swashbuckle.AspNetCore.Filters;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Logger
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateBootstrapLogger();

// Add logger
builder.Host.UseSerilog();

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

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Put JWT Bearer token on textbox below!",

        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    options.OperationFilter<SecurityRequirementsOperationFilter>(true, jwtSecurityScheme.Reference.Id);
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["AuthorizeJWT:Issuer"],
            ValidAudience = builder.Configuration["AuthorizeJWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AuthorizeJWT:Key"]))
        };
    });

builder.Services.AddMassTransit(options =>
{
    options.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("reservation-api", false));

    options.UsingRabbitMq((context, config) =>
    {
        var host = builder.Configuration.GetSection("RabbitMq:Host").Get<string>();

        config.Host(host, h =>
        {
            h.Username(builder.Configuration.GetSection("RabbitMq:Username").Get<string>());
            h.Password(builder.Configuration.GetSection("RabbitMq:Password").Get<string>());
        });

        config.ConfigureEndpoints(context);
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
