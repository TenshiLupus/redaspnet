# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copy csproj and restore as distinct layers
COPY BooksBackend.csproj ./BooksBackend.csproj
RUN dotnet restore "./BooksBackend.csproj"

# copy everything else and build
COPY . .
RUN dotnet publish "./BooksBackend.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

CMD ["dotnet", "booksBackend.dll"]