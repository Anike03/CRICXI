# Use the official ASP.NET Core runtime image as base
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies first (leverages Docker cache)
COPY ["CRICXI.csproj", "./"]
RUN dotnet restore "./CRICXI.csproj"

# Copy everything else and build the project
COPY . .
RUN dotnet publish "./CRICXI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CRICXI.dll"]
