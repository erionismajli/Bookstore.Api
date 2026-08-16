FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Bookstore.Api/Bookstore.Api.csproj src/Bookstore.Api/
RUN dotnet restore src/Bookstore.Api/Bookstore.Api.csproj

COPY . .
RUN dotnet publish src/Bookstore.Api/Bookstore.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 5277

COPY --from=build /app/publish .
RUN mkdir -p /app/keys && chown -R app:app /app
USER app

ENTRYPOINT ["dotnet", "Bookstore.Api.dll"]
