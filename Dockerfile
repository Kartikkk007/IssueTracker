# ==========================================
# STAGE 1: Build & Publish (using .NET 10 SDK)
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Step 1: Copy only project files first (for Docker caching)
COPY ["IssueTracker.Core/IssueTracker.Core.csproj", "IssueTracker.Core/"]
COPY ["IssueTracker.Infrastructure/IssueTracker.Infrastructure.csproj", "IssueTracker.Infrastructure/"]
COPY ["IssueTracker.Application/IssueTracker.Application.csproj", "IssueTracker.Application/"]
COPY ["IssueTracker.UI/IssueTracker.UI.csproj", "IssueTracker.UI/"]

# Step 2: Restore dependencies
RUN dotnet restore "IssueTracker.UI/IssueTracker.UI.csproj"

# Step 3: Copy the entire source code
COPY . .

# Step 4: Publish the UI project in Release mode
WORKDIR "/src/IssueTracker.UI"
RUN dotnet publish "IssueTracker.UI.csproj" -c Release -o /app/publish

# ==========================================
# STAGE 2: Runtime Image (using ASP.NET 10 Runtime)
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copy the compiled output from Stage 1
COPY --from=build /app/publish .

# Expose standard container port
EXPOSE 8080

# Start the application
ENTRYPOINT ["dotnet", "IssueTracker.UI.dll"]