# PowerShell script to reseed database

# Navigate to the API project directory
Set-Location -Path (Split-Path -Parent $PSScriptRoot)

Write-Host "Reseeding database (clear + seed)..." -ForegroundColor Green

try {
    # Run the application with reseed argument
    dotnet run -- reseed
    
    Write-Host "Reseed script completed successfully!" -ForegroundColor Green
}
catch {
    Write-Host "An error occurred: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}