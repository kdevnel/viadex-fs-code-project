# PowerShell script to clear database

# Navigate to the API project directory
Set-Location -Path (Split-Path -Parent $PSScriptRoot)

Write-Host "Clearing database..." -ForegroundColor Green

try {
    # Run the application with clear argument
    dotnet run -- clear
    
    Write-Host "Clear script completed successfully!" -ForegroundColor Green
}
catch {
    Write-Host "An error occurred: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}