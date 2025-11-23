FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /App

COPY ./src/CatalogMicroservice.API ./CatalogMicroservice.API
COPY ./src/CatalogMicroservice.Common ./CatalogMicroservice.Common
COPY ./src/CatalogMicroservice.Service ./CatalogMicroservice.Service
COPY ./src/CatalogMicroservice.Infrastructure ./CatalogMicroservice.Infrastructure
COPY ./src/CheatNuGetSource/InventoryMicroservice.Caller ./CheatNuGetSource/InventoryMicroservice.Caller
COPY ./src/CheatNuGetSource/InventoryMicroservice.Common ./CheatNuGetSource/InventoryMicroservice.Common
RUN dotnet restore ./CatalogMicroservice.API
RUN dotnet publish ./CatalogMicroservice.API -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
EXPOSE 8080
WORKDIR /App
COPY --from=build /App/out .
ENTRYPOINT ["dotnet", "CatalogMicroservice.API.dll"]
