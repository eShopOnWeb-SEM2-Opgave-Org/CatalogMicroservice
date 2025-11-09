using System.Reflection;
using Microsoft.OpenApi.Models;
using CatalogMicroservice.Infrastructure.DependencyInjection;
using CatalogMicroservice.Service.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCors(config =>
{
    config.AddPolicy("default-policy", policy =>
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin()
        );
    config.DefaultPolicyName = "default-policy";
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Catalog API",
        Version = "v1",
        Description = "eShopOnWeb – Catalog microservice"
    });

    var xmlName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlName);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});

var connectionString = Environment.GetEnvironmentVariable(
    "SQL_CONNECTION_STRING",
    EnvironmentVariableTarget.Process
) ?? "";

var databaseName = Environment.GetEnvironmentVariable(
    "SQL_DATABASE_NAME",
    EnvironmentVariableTarget.Process
) ?? "";

builder.Services.AddCatalogServices();
builder.Services.AddCatalogBrandRepository(connectionString, databaseName);
builder.Services.AddCatalogItemRepository(connectionString, databaseName);
builder.Services.AddCatalogTypeRepository(connectionString, databaseName);

builder.Services.AddLogging(config =>
    config
        .AddConsole()
);

var app = builder.Build();

var shouldShowSwagger = Environment.GetEnvironmentVariable(
    "SHOULD_SHOW_SWAGGER",
    EnvironmentVariableTarget.Process
) ?? "";

if (app.Environment.IsDevelopment() || shouldShowSwagger is "true")
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog API v1");
    });
}

var setupDb = Environment.GetEnvironmentVariable("SHOULD_SETUP_DB", EnvironmentVariableTarget.Process) ?? "false";

if (setupDb is not "true")
    Console.WriteLine(" [*] Setup db skipped");

if (setupDb is "true")
{
    using var scope = app.Services.CreateScope();
    scope.SetupCatalogDatabase();
}

app.UseCors("default-policy");

// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
