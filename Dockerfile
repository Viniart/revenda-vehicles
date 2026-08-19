FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY Directory.Build.props Directory.Packages.props ./
COPY src/Revenda.Vehicles.Domain/*.csproj src/Revenda.Vehicles.Domain/
COPY src/Revenda.Vehicles.Application/*.csproj src/Revenda.Vehicles.Application/
COPY src/Revenda.Vehicles.Infrastructure/*.csproj src/Revenda.Vehicles.Infrastructure/
COPY src/Revenda.Vehicles.Api/*.csproj src/Revenda.Vehicles.Api/
RUN dotnet restore src/Revenda.Vehicles.Api/Revenda.Vehicles.Api.csproj

COPY src/ src/
RUN dotnet publish src/Revenda.Vehicles.Api/Revenda.Vehicles.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN adduser --disabled-password --gecos "" --uid 5678 revenda
USER revenda

COPY --from=build /app .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Revenda.Vehicles.Api.dll"]
