# PowerShell script to run database seed

# Navigate to the API project directory
Set-Location -Path (Split-Path -Parent $PSScriptRoot)

Write-Host "Running database seed script..." -ForegroundColor Green

try {
    # Run the application with seed argument
    dotnet run -- seed
    
    Write-Host "Seed script completed successfully!" -ForegroundColor Green
}
catch {
    Write-Host "An error occurred: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}