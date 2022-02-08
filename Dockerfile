FROM tirsvad/dotnet6-debian11:latest AS build

WORKDIR /app

RUN mkdir -p PackageManager

COPY . PackageManager

FROM build AS test

ARG DEBIAN_FRONTEND=noninteractive

WORKDIR /app/PackageManager/src/PackageManager.Tests
RUN dotnet test --logger:"console;verbosity=detailed;"
