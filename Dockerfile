# Multi-stage Docker build for Restaurant Menu API
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY ["RestaurantMenuAPI.sln", "./"]

# Copy project files
COPY ["src/RestaurantMenuAPI/RestaurantMenuAPI.csproj", "src/RestaurantMenuAPI/"]
RUN dotnet restore "src/RestaurantMenuAPI/RestaurantMenuAPI.csproj"

# Copy source code
COPY ["src/RestaurantMenuAPI/", "src/RestaurantMenuAPI/"]
RUN dotnet build "src/RestaurantMenuAPI/RestaurantMenuAPI.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "src/RestaurantMenuAPI/RestaurantMenuAPI.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published application
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "RestaurantMenuAPI.dll"]
