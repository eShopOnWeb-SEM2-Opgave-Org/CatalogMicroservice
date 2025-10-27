FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /App

COPY ./Directory.Packages.props ./
COPY ./src/Microservices/CatalogMicroservice/CatalogMicroservice.API ./CatalogMicroservice.API
COPY ./src/Microservices/CatalogMicroservice/CatalogMicroservice.Common ./CatalogMicroservice.Common
COPY ./src/Microservices/CatalogMicroservice/CatalogMicroservice.Service ./CatalogMicroservice.Service
COPY ./src/Microservices/CatalogMicroservice/CatalogMicroservice.Infrastructure ./CatalogMicroservice.Infrastructure
RUN dotnet restore ./CatalogMicroservice.API
RUN dotnet publish ./CatalogMicroservice.API -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
EXPOSE 8080
WORKDIR /App
COPY --from=build /App/out .
ENTRYPOINT ["dotnet", "CatalogMicroservice.API.dll"]