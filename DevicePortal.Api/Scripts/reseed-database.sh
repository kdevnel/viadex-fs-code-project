#!/bin/bash

# Navigate to the API project directory
cd "$(dirname "$0")/.."

echo "Reseeding database (clear + seed)..."

# Run the application with reseed argument
dotnet run -- reseed

echo "Reseed script completed."