# Use runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base   
# FROM mcr.microsoft.com/dotnet/aspnet:5.0-nanoserver-2004 AS base  

WORKDIR /app

# Install Node and Playwright
RUN apt-get update && \
    apt-get install -y curl gnupg && \
    curl -sL https://deb.nodesource.com/setup_18.x | bash - && \
    apt-get install -y nodejs && \
    npm install -g npm && \
    npm install playwright && \
    npx playwright install

# Copy the published output from GitHub Action
COPY ./publish/ .

# Expose HTTP port
EXPOSE 80

# Set the entry point for the app
ENTRYPOINT ["dotnet", "Uxcheckmate_Main.dll"]