FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props .
COPY src/Shortener.Domain/Shortener.Domain.csproj                   src/Shortener.Domain/
COPY src/Shortener.Application/Shortener.Application.csproj         src/Shortener.Application/
COPY src/Shortener.Contracts/Shortener.Contracts.csproj             src/Shortener.Contracts/
COPY src/Shortener.Infrastructure/Shortener.Infrastructure.csproj   src/Shortener.Infrastructure/
COPY src/Shortener.Redirect/Shortener.Redirect.csproj               src/Shortener.Redirect/
RUN dotnet restore src/Shortener.Redirect/Shortener.Redirect.csproj

COPY src/ src/
RUN dotnet publish src/Shortener.Redirect/Shortener.Redirect.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080

HEALTHCHECK --interval=10s --timeout=3s --start-period=15s --retries=3 \
    CMD curl -f http://localhost:8080/health/live || exit 1

USER $APP_UID
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Shortener.Redirect.dll"]
