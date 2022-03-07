#!/bin/bash
SCRIPTPATH="$( cd "$(dirname "$0")" >/dev/null 2>&1 ; pwd -P )"
PROJECTDIR="$(basename $SCRIPTPATH)"
NAMESPACE="TirsvadCLI.Linux.${PROJECTDIR}"

cd src/${PROJECTDIR}

dotnet clean
dotnet restore
dotnet build
dotnet pack

if [ -f "/srv/programming/NugetPackages/${NAMESPACE}.*" ]; then
  rm /srv/programming/NugetPackages/${NAMESPACE}.*
fi

cp -f bin/Debug/*.nupkg /srv/programming/NugetPackages
cp -f bin/Debug/*.snupkg /srv/programming/NugetPackages

# cd ../${PROJECTDIR}.Tests

# dotnet clean
