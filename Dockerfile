# Use runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# Install Node.js and Playwright dependencies
RUN apt-get update && \
    apt-get install -y curl gnupg && \
    curl -fsSL https://deb.nodesource.com/setup_18.x | bash - && \
    apt-get install -y nodejs && \
    apt-get install -y \
    libnss3 libatk-bridge2.0-0 libx11-xcb1 libxcomposite1 \
    libxdamage1 libxrandr2 libgbm1 libgtk-3-0 libasound2 \
    libpangocairo-1.0-0 libatspi2.0-0 libdrm2 libxshmfence1 \
    libxext6 libxfixes3 libpango-1.0-0 libxcb1 libglu1-mesa \
    fonts-liberation libappindicator1 xdg-utils wget unzip && \
    # Install Playwright and its browsers
    npm install playwright && \
    npx playwright install --with-deps

# Copy the published output from GitHub Action
COPY ./publish/

# Expose HTTP port
EXPOSE 80

ENTRYPOINT ["dotnet", "Uxcheckmate_Main.dll"]
