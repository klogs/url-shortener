FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props .
COPY src/Shortener.Domain/Shortener.Domain.csproj                   src/Shortener.Domain/
COPY src/Shortener.Application/Shortener.Application.csproj         src/Shortener.Application/
COPY src/Shortener.Contracts/Shortener.Contracts.csproj             src/Shortener.Contracts/
COPY src/Shortener.Infrastructure/Shortener.Infrastructure.csproj   src/Shortener.Infrastructure/
COPY src/Shortener.Migrator/Shortener.Migrator.csproj               src/Shortener.Migrator/
RUN dotnet restore src/Shortener.Migrator/Shortener.Migrator.csproj

COPY src/ src/
RUN dotnet publish src/Shortener.Migrator/Shortener.Migrator.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

USER $APP_UID
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Shortener.Migrator.dll"]
