#!/bin/bash

echo "Starting PoBabyTouchGc Build and Test Process..."

# Change to project directory
cd /c/Users/punko/Downloads/PoBabyTouch

# Clean any previous builds
echo "Cleaning previous builds..."
dotnet clean

# Restore packages
echo "Restoring packages..."
dotnet restore

# Build the solution
echo "Building solution..."
dotnet build

# Check if build was successful
if [ $? -eq 0 ]; then
    echo "✅ Build successful!"
    
    # Run tests
    echo "Running unit tests..."
    dotnet test --no-build --verbosity normal
    
    if [ $? -eq 0 ]; then
        echo "✅ Tests passed!"
        
        # Start the application
        echo "Starting application..."
        cd Server/PoBabyTouchGc.Server
        dotnet run
    else
        echo "❌ Tests failed!"
        exit 1
    fi
else
    echo "❌ Build failed!"
    exit 1
fi