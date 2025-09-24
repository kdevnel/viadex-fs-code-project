#!/bin/bash

# Navigate to the API project directory
cd "$(dirname "$0")/.."

echo "Running database seed script..."

# Run the application with seed argument
dotnet run -- seed

echo "Seed script completed."