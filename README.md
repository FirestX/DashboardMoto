# DashboardMoto

A .NET 9 web application for scraping and managing motorcycle listings from various marketplaces. The application features a REST API backend and a web dashboard for viewing and analyzing motorcycle data.

## Features

- **Web Scraping**: Automated scraping from multiple motorcycle marketplaces:
  - AutoScout24.it
  - Mundimoto.com
  - Moto.it
- **REST API**: Endpoints for retrieving and managing motorcycle listings
- **Database Storage**: PostgreSQL database with Entity Framework Core
- **Web Dashboard**: Frontend interface for viewing motorcycle data
- **Logging**: Structured logging with Serilog (console and file output)
- **API Documentation**: Interactive Swagger/OpenAPI documentation

## Requirements

### Software Prerequisites

- **.NET 9.0 SDK** or later ([Download](https://dotnet.microsoft.com/download/dotnet/9.0))
- **PostgreSQL Database** (cloud instance)
- **Chrome** (required for Selenium WebDriver)

### NuGet Packages

The following packages are automatically restored:
- Microsoft.AspNetCore.OpenApi (9.0.10)
- Selenium.WebDriver (4.37.0)
- Selenium.WebDriver.ChromeDriver (141.0.7390.7800)
- Microsoft.EntityFrameworkCore (9.0.10)
- Microsoft.EntityFrameworkCore.Tools (9.0.10)
- Npgsql.EntityFrameworkCore.PostgreSQL (9.0.4)
- Serilog.AspNetCore (9.0.0)
- Swashbuckle.AspNetCore (9.0.6)

## Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/FirestX/DashboardMoto.git
   cd DashboardMoto
   ```

2. **Configure Database Connection**
   
   Edit `appsettings.json` and update the `DefaultConnection` string with your PostgreSQL credentials:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=your-host; Database=your-database; Username=your-username; Password=your-password;"
   }
   ```

3. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

4. **tl;dr**
   ```bash
   git clone https://github.com/FirestX/DashboardMoto.git && cd DashboardMoto && \
   dotnet restore
   ```

## Running the Application

### Backend API

1. Navigate to the main project directory:
   ```bash
   cd DashboardMoto
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. The API will be available at:
   - HTTP: `http://localhost:5189`
   - Swagger UI: `https://localhost:5189/swagger`

### Web Dashboard

1. Navigate to the web dashboard directory:
   ```bash
   cd dashboard-web
   ```

2. Run the dashboard:
   ```bash
   dotnet run
   ```

3. The API will be available at:
   - HTTP: `http://localhost:5206`

4. **tl;dr**
   ```bash
   cd DashboardMoto && dotnet run && \
   cd dashboard-web && dotnet run
   ```

## API Endpoints

### GET `/motorbikes`
Retrieves all motorcycle listings from the database.

**Response**: Array of motorcycle objects with details (model, price, mileage, location, etc.)

### POST `/motorbikes/autoscout24/{nPage}`
Scrapes motorcycle listings from AutoScout24.it

**Parameters**:
- `nPage`: Number of pages to scrape

### POST `/motorbikes/mundimoto/{nPage}`
Scrapes motorcycle listings from Mundimoto.com

**Parameters**:
- `nPage`: Number of pages to scrape

### POST `/motorbikes/motoit/{nPage}`
Scrapes motorcycle listings from Moto.it

**Parameters**:
- `nPage`: Number of pages to scrape

## Project Structure

```
DashboardMoto/
├── Entities/              # Data models and DTOs
├── Persistence/           # Database context and repositories
├── Migrations/            # EF Core database migrations
├── Logs/                  # Application log files
├── dashboard-web/         # Web frontend application
├── Program.cs             # Application entry point and API endpoints
├── WebScraping.cs         # Web scraping logic
├── MotoUtilities.cs       # Utility functions for data processing
└── appsettings.json       # Application configuration
```

### Logging

Logs are written to:
- **Console**: Real-time output during development
- **File**: `Logs/Moto-API-{date}.log` (daily rolling logs)

## Notes

- The application uses Selenium ChromeDriver for web scraping, which requires Chrome to be installed
- Scraping multiple pages runs operations in parallel (max 3 concurrent) for better performance
- The database is seeded automatically on application startup
