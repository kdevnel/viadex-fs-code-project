# Database Management Scripts

This folder contains scripts to populate, clear, and manage the database with sample device data.

## Files

- **SeedDatabase.cs** - The main database management logic (seed, clear, reseed)
- **seed-database.sh** / **seed-database.ps1** - Add sample devices to empty database
- **clear-database.sh** / **clear-database.ps1** - Remove all devices from database
- **reseed-database.sh** / **reseed-database.ps1** - Clear and then seed the database

## Sample Data

The seed script will create 6 sample devices:

1. **iPhone 15 Pro** (Model: A3102) - 45.99/month
2. **Samsung Galaxy S24** (Model: SM-S921B) - 42.50/month
3. **iPad Air** (Model: A2316) - 25.99/month
4. **MacBook Pro 14"** (Model: M3) - 89.99/month
5. **Dell XPS 13** (Model: 9340) - 55.75/month
6. **Google Pixel 8** (Model: GC3VE) - 38.99/month

## Usage

### Seed Database (Add sample devices)
```bash
# Using shell script (macOS/Linux)
./Scripts/seed-database.sh

# Using PowerShell (Windows)
.\Scripts\seed-database.ps1

# Manual execution
dotnet run -- seed
```

### Clear Database (Remove all devices)
```bash
# Using shell script (macOS/Linux)
./Scripts/clear-database.sh

# Using PowerShell (Windows)
.\Scripts\clear-database.ps1

# Manual execution
dotnet run -- clear
```

### Reseed Database (Clear + Seed)
```bash
# Using shell script (macOS/Linux)
./Scripts/reseed-database.sh

# Using PowerShell (Windows)
.\Scripts\reseed-database.ps1

# Manual execution
dotnet run -- reseed
```

## Important Notes

- **Seed**: Only adds devices if the database is empty (won't duplicate data)
- **Clear**: Removes ALL devices from the database (use with caution!)
- **Reseed**: Perfect for testing - clears everything and adds fresh sample data
- Make sure your database connection string is properly configured in `appsettings.json`
- The scripts will create the database if it doesn't exist
- All sample devices have purchase dates in 2024 with realistic monthly pricing

## Requirements

- .NET 9.0 SDK
- Properly configured database connection string
- Entity Framework Core packages (already included in the project)