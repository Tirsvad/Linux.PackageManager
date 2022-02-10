#!/bin/bash

cd src/PackageManager

dotnet clean
dotnet restore
dotnet pack

cp -f /srv/programming/TirsvadCLI/Linux/Dotnet/PackageManager/src/PackageManager/bin/Debug/*.nupkg /srv/programming/NugetPackages
cp -f /srv/programming/TirsvadCLI/Linux/Dotnet/PackageManager/src/PackageManager/bin/Debug/*.snupkg /srv/programming/NugetPackages

cd ../PackageManager.Tests

dotnet clean
dotnet restore
