# Use runtime image
# FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base    # This will pull a Linux-based image
FROM mcr.microsoft.com/dotnet/aspnet:5.0-nanoserver-2004 AS base  

WORKDIR /app

# Install Node.js for Windows (using MSI)
RUN powershell -Command \
    Invoke-WebRequest -Uri https://nodejs.org/dist/v18.15.0/node-v18.15.0-x64.msi -OutFile nodejs.msi; \
    Start-Process msiexec.exe -ArgumentList '/i', 'nodejs.msi', '/quiet', '/norestart' -NoNewWindow -Wait

# Install Playwright and its dependencies
RUN npm install playwright
RUN npx playwright install

# Copy the published output from the GitHub Action (ensure this is published first)
COPY ./publish/ ./ 

# Expose HTTP port
EXPOSE 80

# Set the entry point for the app
ENTRYPOINT ["dotnet", "Uxcheckmate_Main.dll"]