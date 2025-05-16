FROM mcr.microsoft.com/playwright:v1.52.0-jammy 

WORKDIR /app
COPY . .

RUN npm ci || npm install

EXPOSE 5000
CMD ["npm", "start"]
