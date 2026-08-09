FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Shortener.Domain/Shortener.Domain.csproj                   src/Shortener.Domain/
COPY src/Shortener.Application/Shortener.Application.csproj         src/Shortener.Application/
COPY src/Shortener.Contracts/Shortener.Contracts.csproj             src/Shortener.Contracts/
COPY src/Shortener.Infrastructure/Shortener.Infrastructure.csproj   src/Shortener.Infrastructure/
COPY src/Shortener.Api/Shortener.Api.csproj                         src/Shortener.Api/
RUN dotnet restore src/Shortener.Api/Shortener.Api.csproj

COPY src/ src/
RUN dotnet publish src/Shortener.Api/Shortener.Api.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080

HEALTHCHECK --interval=15s --timeout=5s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:8080/health/live || exit 1

USER $APP_UID
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Shortener.Api.dll"]
