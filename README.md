# URL Shortener with Analytics Dashboard

A full-stack URL shortening service built with ASP.NET Core backend and modern web frontend, featuring comprehensive analytics and user management.

## 🚀 Features

### Core Functionality
- **URL Shortening**: Create short, memorable URLs from long links
- **Click Tracking**: Monitor clicks with detailed analytics
- **User Authentication**: Secure JWT-based authentication system
- **Rate Limiting**: Protection against abuse with configurable limits
- **Input Validation**: Robust validation using FluentValidation

### Dashboard Features
- **Real-time Analytics**: Live click tracking and statistics
- **User Dashboard**: Personalized overview of all user's links
- **Link Management**: View and manage all shortened URLs
- **Time-based Analytics**: Today, week, and month click statistics
- **Top Performing Links**: Identify your most successful URLs
- **Daily Trends**: 30-day click history visualization

### Technical Features
- **MongoDB Integration**: Scalable NoSQL database
- **JWT Authentication**: Secure token-based authentication
- **CORS Support**: Cross-origin request handling
- **Docker Support**: Containerized deployment
- **Modern UI**: Responsive, gradient-based design

## 🏗️ Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Frontend      │    │   Backend       │    │   Database      │
│   (HTML/CSS/JS) │◄──►│   (ASP.NET Core)│◄──►│   (MongoDB)     │
│                 │    │                 │    │                 │
│ • Dashboard     │    │ • REST API      │    │ • Users         │
│ • Authentication│    │ • JWT Auth      │    │ • Short URLs    │
│ • Analytics     │    │ • Rate Limiting │    │ • Click Data    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## 📋 Prerequisites

- **.NET 9.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/9.0)
- **MongoDB** - [Download here](https://www.mongodb.com/try/download/community)
- **Modern Web Browser** - Chrome, Firefox, Safari, or Edge
- **PowerShell** (for Windows testing scripts)

## 🛠️ Installation & Setup

### 1. Clone and Navigate
```bash
cd backend
```

### 2. Configure Database
Update `Urlshortener.Api/appsettings.json`:
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "urlshortener"
  }
}
```

### 3. Build and Run Backend
```bash
dotnet build
dotnet run --project Urlshortener.Api
```

The API will be available at `http://localhost:5084`

### 4. Test the System
```powershell
.\test-dashboard.ps1
```

### 5. Open Dashboard
Open `dashboard.html` in your web browser and login with:
- **Username:** `dashboarduser`
- **Password:** `TestPass123`

## 📚 API Documentation

### Authentication Endpoints

#### POST `/register`
Register a new user account.

**Request:**
```json
{
  "username": "newuser",
  "password": "SecurePass123"
}
```

**Response:**
```json
{
  "message": "Registration successful."
}
```

#### POST `/login`
Authenticate and receive JWT token.

**Request:**
```json
{
  "username": "newuser",
  "password": "SecurePass123"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

#### GET `/me`
Get current user information.

**Headers:** `Authorization: Bearer <token>`

**Response:**
```json
{
  "id": "507f1f77bcf86cd799439011",
  "username": "newuser"
}
```

### URL Management Endpoints

#### POST `/shorten`
Create a new shortened URL.

**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "originalUrl": "https://www.example.com/very/long/url"
}
```

**Response:**
```json
{
  "shortUrl": "http://localhost:5084/r/abc123"
}
```

#### GET `/r/{shortCode}`
Redirect to original URL (tracks analytics).

**Response:** HTTP 302 Redirect

### Analytics Endpoints

#### GET `/dashboard`
Get comprehensive dashboard summary.

**Headers:** `Authorization: Bearer <token>`

**Response:**
```json
{
  "totalLinks": 5,
  "totalClicks": 25,
  "clicksToday": 3,
  "clicksThisWeek": 12,
  "clicksThisMonth": 25,
  "topLinks": [
    {
      "id": "507f1f77bcf86cd799439011",
      "shortCode": "abc123",
      "originalUrl": "https://www.google.com",
      "shortUrl": "http://localhost:5084/r/abc123",
      "createdAt": "2024-01-15T10:30:00Z",
      "totalClicks": 10,
      "clicksToday": 2,
      "clicksThisWeek": 5,
      "clicksThisMonth": 10
    }
  ],
  "dailyClicks": [
    {
      "date": "2024-01-15T00:00:00Z",
      "clicks": 3
    }
  ]
}
```

#### GET `/dashboard/links`
Get all user's links with analytics.

**Headers:** `Authorization: Bearer <token>`

**Response:**
```json
[
  {
    "id": "507f1f77bcf86cd799439011",
    "shortCode": "abc123",
    "originalUrl": "https://www.google.com",
    "shortUrl": "http://localhost:5084/r/abc123",
    "createdAt": "2024-01-15T10:30:00Z",
    "totalClicks": 10,
    "clicksToday": 2,
    "clicksThisWeek": 5,
    "clicksThisMonth": 10
  }
]
```

#### GET `/dashboard/links/{shortCode}`
Get detailed analytics for a specific link.

**Headers:** `Authorization: Bearer <token>`

**Response:**
```json
{
  "id": "507f1f77bcf86cd799439011",
  "shortCode": "abc123",
  "originalUrl": "https://www.google.com",
  "shortUrl": "http://localhost:5084/r/abc123",
  "createdAt": "2024-01-15T10:30:00Z",
  "totalClicks": 10,
  "clicks": [
    {
      "id": "507f1f77bcf86cd799439012",
      "shortUrlId": "507f1f77bcf86cd799439011",
      "timestamp": "2024-01-15T14:30:00Z",
      "ipAddress": "192.168.1.1",
      "userAgent": "Mozilla/5.0..."
    }
  ],
  "dailyClicks": [
    {
      "date": "2024-01-15T00:00:00Z",
      "clicks": 3
    }
  ]
}
```

#### GET `/analytics/{shortCode}`
Get raw click analytics for a link.

**Headers:** `Authorization: Bearer <token>`

**Response:**
```json
[
  {
    "id": "507f1f77bcf86cd799439012",
    "shortUrlId": "507f1f77bcf86cd799439011",
    "timestamp": "2024-01-15T14:30:00Z",
    "ipAddress": "192.168.1.1",
    "userAgent": "Mozilla/5.0..."
  }
]
```

## 🎨 Frontend Features

### Dashboard Interface
- **Modern Design**: Clean, gradient-based UI with smooth animations
- **Responsive Layout**: Works perfectly on desktop and mobile devices
- **Tab Navigation**: Easy switching between Overview and My Links
- **Real-time Updates**: Live data loading from the API
- **Error Handling**: User-friendly error messages and loading states

### User Experience
- **Persistent Login**: JWT tokens stored in localStorage
- **Auto-refresh**: Automatic data updates
- **Interactive Elements**: Hover effects and smooth transitions
- **Cross-browser Support**: Works in all modern browsers

## 🔧 Configuration

### Rate Limiting
Configure in `appsettings.json`:
```json
{
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "StackBlockedRequests": false,
    "RealIpHeader": "X-Real-IP",
    "ClientIdHeader": "X-ClientId",
    "HttpStatusCode": 429,
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 100
      }
    ]
  }
}
```

### JWT Settings
Update JWT secret in `Program.cs`:
```csharp
IssuerSigningKey = new SymmetricSecurityKey(
    Encoding.ASCII.GetBytes("YOUR_SECURE_SECRET_KEY_HERE")
)
```

## 🐳 Docker Deployment

### Build and Run with Docker
```bash
# Build the image
docker build -t urlshortener-api .

# Run the container
docker run -p 8080:8080 urlshortener-api
```

### Docker Compose
```bash
# Start all services
docker-compose up -d

# Stop all services
docker-compose down
```

## 🧪 Testing

### Automated Testing
```powershell
# Run the test script
.\test-dashboard.ps1
```

### Manual Testing with Postman
1. Import `UrlShortener_API.postman_collection.json`
2. Set environment variable `baseUrl` to `http://localhost:5084`
3. Run the collection tests

### Browser Testing
1. Open `dashboard.html` in your browser
2. Login with test credentials
3. Test all dashboard features

## 📊 Data Models

### User
```csharp
public class User
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
}
```

### ShortUrl
```csharp
public class ShortUrl
{
    public string Id { get; set; }
    public string OriginalUrl { get; set; }
    public string ShortCode { get; set; }
    public string UserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### ClickAnalytics
```csharp
public class ClickAnalytics
{
    public string Id { get; set; }
    public string ShortUrlId { get; set; }
    public DateTime Timestamp { get; set; }
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
}
```

## 🔒 Security Features

- **JWT Authentication**: Secure token-based authentication
- **Password Hashing**: BCrypt password hashing
- **Rate Limiting**: Protection against abuse
- **Input Validation**: Comprehensive validation using FluentValidation
- **CORS Configuration**: Secure cross-origin request handling
- **User Isolation**: Users can only access their own data

## 🚀 Performance Optimizations

- **Efficient MongoDB Queries**: Optimized database operations
- **In-Memory Caching**: Rate limiting uses memory cache
- **Async Operations**: Non-blocking API calls
- **Connection Pooling**: MongoDB connection optimization

## 🔮 Future Enhancements

- **Real-time Updates**: WebSocket support for live dashboard
- **Advanced Analytics**: Geographic data, device types, referral sources
- **Export Features**: CSV/PDF export of analytics data
- **Custom Domains**: Support for custom short URLs
- **QR Code Generation**: Automatic QR code generation
- **Bulk Operations**: Create and manage multiple links
- **Link Expiration**: Set expiration dates for links
- **Password Protection**: Password-protected short URLs
- **API Rate Limiting**: Per-user rate limiting
- **Webhook Support**: Real-time notifications

## 🐛 Troubleshooting

### Common Issues

1. **CORS Errors**
   - Ensure backend is running on correct port
   - Check CORS configuration in Program.cs

2. **Authentication Failures**
   - Verify JWT token validity
   - Check token expiration (7 days default)

3. **Database Connection**
   - Ensure MongoDB is running
   - Verify connection string in appsettings.json

4. **Port Conflicts**
   - Check if port 5084 is available
   - Use different port: `dotnet run --urls=http://localhost:8080`

### Debug Mode
```bash
set ASPNETCORE_ENVIRONMENT=Development
dotnet run --project Urlshortener.Api
```

## 📝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Support

If you encounter any issues or have questions:

1. Check the troubleshooting section above
2. Review the API documentation
3. Test with the provided scripts
4. Open an issue on GitHub

## 📈 Analytics Dashboard Screenshots

The dashboard provides:
- **Overview Tab**: Key metrics and top performing links
- **My Links Tab**: Complete list with individual analytics
- **Real-time Statistics**: Live click tracking
- **Beautiful UI**: Modern, responsive design

---

**Built with ❤️ using ASP.NET Core, MongoDB, and modern web technologies** 