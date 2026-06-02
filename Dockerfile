# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY BOOKORIA/BOOKORIA.csproj BOOKORIA/
RUN dotnet restore BOOKORIA/BOOKORIA.csproj
COPY . .
RUN dotnet publish BOOKORIA/BOOKORIA.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV DOTNET_USE_POLLING_FILE_WATCHER=true

COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "BOOKORIA.dll"]
