# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files
COPY SaveApp.sln ./
COPY SaveApp/*.csproj ./SaveApp/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY SaveApp/. ./SaveApp/

# Build and publish the app
WORKDIR /app/SaveApp
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/SaveApp/out ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "SaveApp.dll"]

