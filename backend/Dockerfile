# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy solution and project files first for layer caching
COPY NEmaris.slnx ./
COPY src/NEmaris.API/NEmaris.API.csproj             src/NEmaris.API/
COPY src/NEmaris.Application/NEmaris.Application.csproj src/NEmaris.Application/
COPY src/NEmaris.Domain/NEmaris.Domain.csproj       src/NEmaris.Domain/
COPY src/NEmaris.Infrastructure/NEmaris.Infrastructure.csproj src/NEmaris.Infrastructure/
COPY tests/NEmaris.Tests/NEmaris.Tests.csproj       tests/NEmaris.Tests/

RUN dotnet restore

# Copy everything else and publish
COPY . .
RUN dotnet publish src/NEmaris.API/NEmaris.API.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "NEmaris.API.dll"]
