# Use a .NET 9 SDK base image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set the working directory in the container
WORKDIR /app 

# Copy solution files and restore dependencies
COPY ./*.sln ./
COPY PersonalBudgetManager.Api/PersonalBudgetManager.Api.csproj PersonalBudgetManager.Api/
COPY PersonaButgetManager.Tests/PersonaButgetManager.Tests.csproj PersonaButgetManager.Tests/

RUN dotnet restore PersonalBudgetManager.Api/PersonalBudgetManager.Api.csproj

# Copy the rest of the code and build the application
COPY . ./
WORKDIR /app/PersonalBudgetManager.Api
RUN dotnet publish -c Release -o out

# Use a .NET Runtime base image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:9.0

# Set the working directory in the container
WORKDIR /app

# Copy artifacts from the build stage
COPY --from=build /app/PersonalBudgetManager.Api/out ./

# Expose the ports in the container (adjust according to your application's ports)
EXPOSE 80
EXPOSE 443 
EXPOSE 1433

# Command to run the application
ENTRYPOINT ["dotnet", "PersonalBudgetManager.Api.dll"]
