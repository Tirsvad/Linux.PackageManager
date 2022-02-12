FROM tirsvad/debian_11-dotnet_6:latest AS build

ARG DEBIAN_FRONTEND=noninteractive

WORKDIR /app

RUN mkdir -p PackageManager

COPY . PackageManager

WORKDIR /app/PackageManager/src/PackageManager

RUN dotnet restore
RUN dotnet build
RUN dotnet pack
RUN cp /app/PackageManager/src/PackageManager/bin/Debug/*.nupkg /srv/Nuget/
RUN cp /app/PackageManager/src/PackageManager/bin/Debug/*.snupkg /srv/Nuget/

FROM build AS test

WORKDIR /app/PackageManager/src/PackageManager.Tests
RUN dotnet test --logger:"console;verbosity=detailed;"
