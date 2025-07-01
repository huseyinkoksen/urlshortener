# Deployment Guide - URL Shortener Dashboard

This guide covers deploying your URL Shortener application to various environments, from local development to production.

## 🏠 Local Development

### Prerequisites
- .NET 9.0 SDK
- MongoDB Community Edition
- Modern web browser

### Quick Start
```bash
# 1. Start MongoDB
mongod

# 2. Start the API
cd backend
dotnet run --project Urlshortener.Api

# 3. Open dashboard
# Open dashboard.html in your browser
```

## 🐳 Docker Deployment

### Single Container
```bash
# Build the image
docker build -t urlshortener-api .

# Run the container
docker run -d \
  --name urlshortener-api \
  -p 8080:8080 \
  -e MongoDbSettings__ConnectionString=mongodb://host.docker.internal:27017 \
  -e MongoDbSettings__DatabaseName=urlshortener \
  urlshortener-api
```

### Docker Compose (Recommended)
```yaml
# docker-compose.yml
version: '3.8'
services:
  mongodb:
    image: mongo:7.0
    container_name: urlshortener-mongodb
    ports:
      - "27017:27017"
    volumes:
      - mongodb_data:/data/db
    environment:
      MONGO_INITDB_ROOT_USERNAME: admin
      MONGO_INITDB_ROOT_PASSWORD: password

  api:
    build: .
    container_name: urlshortener-api
    ports:
      - "8080:8080"
    depends_on:
      - mongodb
    environment:
      - MongoDbSettings__ConnectionString=mongodb://admin:password@mongodb:27017
      - MongoDbSettings__DatabaseName=urlshortener
      - ASPNETCORE_ENVIRONMENT=Production

volumes:
  mongodb_data:
```

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose down
```

## ☁️ Cloud Deployment

### Azure App Service

#### 1. Prepare for Azure
```bash
# Publish the application
dotnet publish -c Release -o ./publish

# Create deployment package
cd publish
zip -r ../urlshortener.zip .
```

#### 2. Azure CLI Deployment
```bash
# Login to Azure
az login

# Create resource group
az group create --name urlshortener-rg --location eastus

# Create App Service plan
az appservice plan create \
  --name urlshortener-plan \
  --resource-group urlshortener-rg \
  --sku B1

# Create web app
az webapp create \
  --name urlshortener-api \
  --resource-group urlshortener-rg \
  --plan urlshortener-plan \
  --runtime "DOTNETCORE:9.0"

# Deploy the application
az webapp deployment source config-zip \
  --resource-group urlshortener-rg \
  --name urlshortener-api \
  --src urlshortener.zip

# Configure environment variables
az webapp config appsettings set \
  --resource-group urlshortener-rg \
  --name urlshortener-api \
  --settings \
    MongoDbSettings__ConnectionString="your_mongodb_connection_string" \
    MongoDbSettings__DatabaseName="urlshortener"
```

#### 3. Azure Cosmos DB (MongoDB API)
```bash
# Create Cosmos DB account
az cosmosdb create \
  --name urlshortener-cosmos \
  --resource-group urlshortener-rg \
  --kind MongoDB

# Get connection string
az cosmosdb keys list \
  --name urlshortener-cosmos \
  --resource-group urlshortener-rg \
  --type connection-strings
```

### AWS Elastic Beanstalk

#### 1. Prepare Application
```bash
# Create deployment package
dotnet publish -c Release -o ./publish
cd publish
zip -r ../urlshortener-aws.zip .
```

#### 2. AWS CLI Deployment
```bash
# Create Elastic Beanstalk application
aws elasticbeanstalk create-application \
  --application-name urlshortener-api

# Create environment
aws elasticbeanstalk create-environment \
  --application-name urlshortener-api \
  --environment-name urlshortener-prod \
  --solution-stack-name "64bit Amazon Linux 2 v2.4.0 running .NET Core"

# Deploy application
aws elasticbeanstalk create-application-version \
  --application-name urlshortener-api \
  --version-label v1.0.0 \
  --source-bundle S3Bucket="your-bucket",S3Key="urlshortener-aws.zip"

aws elasticbeanstalk update-environment \
  --environment-name urlshortener-prod \
  --version-label v1.0.0
```

#### 3. Environment Variables
```bash
# Set environment variables
aws elasticbeanstalk update-environment \
  --environment-name urlshortener-prod \
  --option-settings \
    Namespace=aws:elasticbeanstalk:application:environment,OptionName=MongoDbSettings__ConnectionString,Value="your_mongodb_connection_string" \
    Namespace=aws:elasticbeanstalk:application:environment,OptionName=MongoDbSettings__DatabaseName,Value="urlshortener"
```

### Google Cloud Platform

#### 1. App Engine Deployment
```yaml
# app.yaml
runtime: aspnetcore
env: flex

env_variables:
  MongoDbSettings__ConnectionString: "your_mongodb_connection_string"
  MongoDbSettings__DatabaseName: "urlshortener"
  ASPNETCORE_ENVIRONMENT: "Production"

automatic_scaling:
  min_num_instances: 1
  max_num_instances: 10
```

```bash
# Deploy to App Engine
gcloud app deploy app.yaml
```

#### 2. Cloud Run Deployment
```bash
# Build and push to Container Registry
docker build -t gcr.io/your-project/urlshortener-api .
docker push gcr.io/your-project/urlshortener-api

# Deploy to Cloud Run
gcloud run deploy urlshortener-api \
  --image gcr.io/your-project/urlshortener-api \
  --platform managed \
  --region us-central1 \
  --allow-unauthenticated \
  --set-env-vars MongoDbSettings__ConnectionString="your_mongodb_connection_string",MongoDbSettings__DatabaseName="urlshortener"
```

## 🔧 Production Configuration

### Environment Variables
```bash
# Required
MongoDbSettings__ConnectionString=mongodb://username:password@host:port/database
MongoDbSettings__DatabaseName=urlshortener

# Optional
ASPNETCORE_ENVIRONMENT=Production
JWT_SECRET=your_secure_jwt_secret_key_here
```

### Security Considerations

#### 1. JWT Secret
```csharp
// Program.cs - Update with secure secret
IssuerSigningKey = new SymmetricSecurityKey(
    Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET"))
)
```

#### 2. HTTPS Configuration
```csharp
// Program.cs
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
```

#### 3. CORS Configuration
```csharp
// Program.cs - Restrict to your domain
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://yourdomain.com")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

### Database Configuration

#### MongoDB Atlas (Cloud)
```bash
# Connection string format
mongodb+srv://username:password@cluster.mongodb.net/urlshortener?retryWrites=true&w=majority
```

#### Local MongoDB with Authentication
```bash
# Enable authentication
mongod --auth

# Create admin user
use admin
db.createUser({
  user: "admin",
  pwd: "secure_password",
  roles: ["userAdminAnyDatabase", "dbAdminAnyDatabase", "readWriteAnyDatabase"]
})

# Create application user
use urlshortener
db.createUser({
  user: "app_user",
  pwd: "app_password",
  roles: ["readWrite"]
})
```

## 📊 Monitoring and Logging

### Application Insights (Azure)
```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry();
```

### Health Checks
```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddMongoDb(connectionString);

app.MapHealthChecks("/health");
```

### Logging Configuration
```json
// appsettings.Production.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## 🔄 CI/CD Pipeline

### GitHub Actions
```yaml
# .github/workflows/deploy.yml
name: Deploy to Production

on:
  push:
    branches: [ main ]

jobs:
  deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 9.0.x
    
    - name: Build
      run: dotnet build --configuration Release
    
    - name: Test
      run: dotnet test --no-build --verbosity normal
    
    - name: Deploy to Azure
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'urlshortener-api'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ./Urlshortener.Api/bin/Release/net9.0/publish/
```

### Azure DevOps
```yaml
# azure-pipelines.yml
trigger:
- main

pool:
  vmImage: 'ubuntu-latest'

variables:
  solution: '**/*.sln'
  buildPlatform: 'Any CPU'
  buildConfiguration: 'Release'

steps:
- task: DotNetCoreCLI@2
  inputs:
    command: 'restore'
    projects: '$(solution)'

- task: DotNetCoreCLI@2
  inputs:
    command: 'build'
    projects: '$(solution)'
    arguments: '--configuration $(buildConfiguration)'

- task: DotNetCoreCLI@2
  inputs:
    command: 'publish'
    publishWebProjects: true
    arguments: '--configuration $(buildConfiguration) --output $(Build.ArtifactStagingDirectory)'
    zipAfterPublish: true

- task: PublishBuildArtifacts@1
  inputs:
    PathtoPublish: '$(Build.ArtifactStagingDirectory)'
    ArtifactName: 'drop'
    publishLocation: 'Container'
```

## 🚀 Performance Optimization

### Database Indexing
```javascript
// MongoDB indexes
db.shorturls.createIndex({ "shortCode": 1 }, { unique: true })
db.shorturls.createIndex({ "userId": 1 })
db.clickanalytics.createIndex({ "shortUrlId": 1 })
db.clickanalytics.createIndex({ "timestamp": -1 })
```

### Caching
```csharp
// Program.cs
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();

app.UseResponseCaching();
```

### Load Balancing
```nginx
# nginx.conf
upstream urlshortener {
    server localhost:8080;
    server localhost:8081;
    server localhost:8082;
}

server {
    listen 80;
    server_name yourdomain.com;
    
    location / {
        proxy_pass http://urlshortener;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

## 🔍 Troubleshooting

### Common Issues

#### 1. Database Connection
```bash
# Test MongoDB connection
mongosh "mongodb://username:password@host:port/database"
```

#### 2. Port Conflicts
```bash
# Check port usage
netstat -ano | findstr :8080

# Kill process
taskkill /PID <process_id> /F
```

#### 3. Environment Variables
```bash
# Verify environment variables
echo $MongoDbSettings__ConnectionString
echo $ASPNETCORE_ENVIRONMENT
```

#### 4. SSL/TLS Issues
```bash
# Test HTTPS
curl -k https://yourdomain.com/health
```

## 📈 Scaling Considerations

### Horizontal Scaling
- Use load balancer for multiple instances
- Implement session affinity if needed
- Use distributed caching (Redis)

### Database Scaling
- MongoDB replica sets for read scaling
- MongoDB sharding for write scaling
- Consider read replicas for analytics

### CDN Integration
- Serve static files (dashboard.html) via CDN
- Cache API responses where appropriate
- Use edge locations for global performance

---

**🎯 Your URL Shortener is now ready for production deployment!** 