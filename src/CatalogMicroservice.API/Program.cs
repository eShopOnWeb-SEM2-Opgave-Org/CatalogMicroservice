using System.Reflection;
using Microsoft.OpenApi.Models;
using CatalogMicroservice.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

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

builder.Services.AddCatalogBrandRepository(connectionString, databaseName);
builder.Services.AddCatalogItemRepository(connectionString, databaseName);
builder.Services.AddCatalogTypeRepository(connectionString, databaseName);

builder.Services.AddLogging();

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

using (var scope = app.Services.CreateScope())
{
    scope.SetupCatalogDatabase();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
