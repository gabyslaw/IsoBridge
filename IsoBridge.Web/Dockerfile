# ============================
#   STAGE 1: Build
# ============================
FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /app

ENV NUGET_PACKAGES=/root/.nuget/packages
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

# Copy solution and csproj files
COPY *.sln .
COPY IsoBridge.Core/*.csproj IsoBridge.Core/
COPY IsoBridge.Infrastructure/*.csproj IsoBridge.Infrastructure/
COPY IsoBridge.ISO8583/*.csproj IsoBridge.ISO8583/
COPY IsoBridge.Web/*.csproj IsoBridge.Web/

# Restore at the project level
RUN dotnet restore IsoBridge.Web/IsoBridge.Web.csproj

# Copy the rest of the code
COPY . .

# Clean up any Windows build leftovers
RUN find . -type d \( -name bin -o -name obj \) -exec rm -rf {} +

WORKDIR /app/IsoBridge.Web
RUN dotnet restore

# Build & publish
RUN dotnet build -c Release --no-restore
RUN dotnet publish -c Release -o /out --no-restore

# ============================
#   STAGE 2: Runtime
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine
WORKDIR /app
COPY --from=build /out .
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "IsoBridge.Web.dll"]
