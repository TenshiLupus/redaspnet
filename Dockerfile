# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copy csproj and restore as distinct layers
COPY booksBackend.csproj .
RUN dotnet restore

# copy everything else and build
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# copy published app
COPY --from=build /app/publish .

# Kestrel port binding is handled in Program.cs via PORT env var
# so we just start the app
CMD ["dotnet", "booksBackend.dll"]