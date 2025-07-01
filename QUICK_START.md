# Quick Start Guide - URL Shortener Dashboard

Get your URL shortener with analytics dashboard up and running in 5 minutes!

## 🚀 Prerequisites

- **.NET 9.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/9.0)
- **MongoDB** - [Download here](https://www.mongodb.com/try/download/community)
- **Modern Web Browser** (Chrome, Firefox, Safari, Edge)

## ⚡ Quick Setup (5 minutes)

### 1. Start MongoDB
```bash
# Windows (if installed as service)
# MongoDB should start automatically

# Or start manually
"C:\Program Files\MongoDB\Server\7.0\bin\mongod.exe"
```

### 2. Start the Backend
```bash
cd backend
dotnet run --project Urlshortener.Api
```

You should see:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5084
```

### 3. Test the System
```powershell
# In a new PowerShell window
.\test-dashboard.ps1
```

### 4. Open Dashboard
1. Open `dashboard.html` in your browser
2. Login with:
   - **Username:** `dashboarduser`
   - **Password:** `TestPass123`

## 🎯 What You'll See

### Dashboard Overview
- **Total Links**: Number of URLs you've created
- **Total Clicks**: Combined clicks across all your links
- **Clicks Today**: Clicks recorded today
- **Clicks This Week**: Clicks in the last 7 days
- **Top Performing Links**: Your most successful URLs

### My Links Tab
- Complete list of all your shortened URLs
- Individual analytics for each link
- Click statistics (total, today, week, month)

## 🔧 Create Your First Short URL

### Using the API
```bash
# Login to get token
curl -X POST http://localhost:5084/login \
  -H "Content-Type: application/json" \
  -d '{"username": "dashboarduser", "password": "TestPass123"}'

# Create short URL (replace <token> with your actual token)
curl -X POST http://localhost:5084/shorten \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"originalUrl": "https://www.google.com"}'
```

### Using Postman
1. Import `UrlShortener_API.postman_collection.json`
2. Set `baseUrl` environment variable to `http://localhost:5084`
3. Run "Login User" to get token
4. Run "Create Short URL" to create your first link

## 📊 View Analytics

### Dashboard Summary
```bash
curl -X GET http://localhost:5084/dashboard \
  -H "Authorization: Bearer <token>"
```

### Individual Link Analytics
```bash
curl -X GET http://localhost:5084/dashboard/links/abc123 \
  -H "Authorization: Bearer <token>"
```

## 🐳 Docker Alternative

If you prefer Docker:

```bash
# Build and run with Docker Compose
docker-compose up -d

# Or build manually
docker build -t urlshortener-api .
docker run -p 8080:8080 urlshortener-api
```

## 🔍 Troubleshooting

### Backend Won't Start
```bash
# Check if port is in use
netstat -ano | findstr :5084

# Kill process if needed
taskkill /PID <process_id> /F
```

### MongoDB Connection Issues
```bash
# Check if MongoDB is running
netstat -ano | findstr :27017

# Start MongoDB manually
"C:\Program Files\MongoDB\Server\7.0\bin\mongod.exe"
```

### Dashboard Shows "Network Error"
1. Ensure backend is running on `http://localhost:5084`
2. Check browser console (F12) for CORS errors
3. Verify the API_BASE URL in dashboard.html

### Authentication Issues
```bash
# Check if user exists
curl -X POST http://localhost:5084/register \
  -H "Content-Type: application/json" \
  -d '{"username": "testuser", "password": "TestPass123"}'
```

## 📱 Test Credentials

| Username | Password | Purpose |
|----------|----------|---------|
| `dashboarduser` | `TestPass123` | Test account with sample data |
| `testuser` | `TestPass123` | Create your own account |

## 🎨 Customization

### Change Port
```bash
dotnet run --project Urlshortener.Api --urls=http://localhost:8080
```

### Update Dashboard URL
Edit `dashboard.html`:
```javascript
const API_BASE = 'http://localhost:8080'; // Change to your port
```

### Custom JWT Secret
Edit `Program.cs`:
```csharp
IssuerSigningKey = new SymmetricSecurityKey(
    Encoding.ASCII.GetBytes("YOUR_CUSTOM_SECRET_KEY")
)
```

## 📈 Next Steps

1. **Create More Links**: Test with different URLs
2. **View Analytics**: Monitor click patterns
3. **Customize UI**: Modify dashboard.html styling
4. **Add Features**: Implement bulk operations, QR codes, etc.
5. **Deploy**: Move to production environment

## 🆘 Need Help?

1. **Check Logs**: Look at console output for errors
2. **Test Endpoints**: Use the test script or Postman
3. **Verify Setup**: Ensure MongoDB and .NET are installed
4. **Check Ports**: Confirm no port conflicts

## 📚 Documentation

- **Full README**: `README.md` - Complete project documentation
- **API Docs**: `API_DOCUMENTATION.md` - Detailed API reference
- **Dashboard Guide**: `README-Dashboard.md` - Dashboard-specific features

---

**🎉 You're all set! Your URL shortener with analytics dashboard is ready to use.** 