# Create build environment
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy csproj and restore project
COPY *.sln .
COPY src/Application/*.csproj ./src/Application/
COPY src/Domain/*.csproj ./src/Domain/
COPY src/Infrastructure/*.csproj ./src/Infrastructure/
COPY src/Presentation/*.csproj ./src/Presentation/
COPY tests/Application.UnitTests/*.csproj ./tests/Application.UnitTests/
RUN dotnet restore

# Copy the rest and build
COPY . .
RUN dotnet publish -c Release -o out

# Create runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "Presentation.dll"]