#!/bin/bash

# Navigate to the API project directory
cd "$(dirname "$0")/.."

echo "Clearing database..."

# Run the application with clear argument
dotnet run -- clear

echo "Clear script completed."