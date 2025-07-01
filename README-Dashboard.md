# URL Shortener Dashboard

A comprehensive user dashboard for the URL Shortener application that provides analytics and management features for shortened URLs.

## Features

### 🔐 Authentication
- User registration and login
- JWT-based authentication
- Secure token storage

### 📊 Dashboard Overview
- **Total Links**: Number of URLs created by the user
- **Total Clicks**: Combined clicks across all user's links
- **Clicks Today**: Clicks recorded today
- **Clicks This Week**: Clicks recorded in the last 7 days
- **Top Performing Links**: Links with the highest click counts

### 🔗 Link Management
- View all user's shortened URLs
- Individual link analytics
- Click tracking with timestamps
- Daily click statistics for the last 30 days

### 📈 Analytics
- Real-time click tracking
- Time-based analytics (today, week, month)
- IP address and user agent tracking
- Detailed click history

## API Endpoints

### Dashboard Endpoints

#### GET `/dashboard`
Returns a comprehensive dashboard summary for the authenticated user.

**Headers:**
```
Authorization: Bearer <jwt_token>
```

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
      "shortUrl": "http://localhost:8080/r/abc123",
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
Returns all links created by the authenticated user with analytics.

**Headers:**
```
Authorization: Bearer <jwt_token>
```

**Response:**
```json
[
  {
    "id": "507f1f77bcf86cd799439011",
    "shortCode": "abc123",
    "originalUrl": "https://www.google.com",
    "shortUrl": "http://localhost:8080/r/abc123",
    "createdAt": "2024-01-15T10:30:00Z",
    "totalClicks": 10,
    "clicksToday": 2,
    "clicksThisWeek": 5,
    "clicksThisMonth": 10
  }
]
```

#### GET `/dashboard/links/{shortCode}`
Returns detailed analytics for a specific link.

**Headers:**
```
Authorization: Bearer <jwt_token>
```

**Response:**
```json
{
  "id": "507f1f77bcf86cd799439011",
  "shortCode": "abc123",
  "originalUrl": "https://www.google.com",
  "shortUrl": "http://localhost:8080/r/abc123",
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

## Frontend Dashboard

The dashboard includes a modern, responsive web interface with:

### 🎨 Design Features
- **Modern UI**: Clean, gradient-based design
- **Responsive Layout**: Works on desktop and mobile devices
- **Interactive Elements**: Hover effects and smooth transitions
- **Tab Navigation**: Easy switching between overview and links

### 📱 User Interface
- **Login/Register Forms**: Secure authentication
- **Dashboard Overview**: Key metrics at a glance
- **Link Management**: View and manage all shortened URLs
- **Real-time Updates**: Live data from the API

### 🔧 Technical Features
- **Local Storage**: Persistent authentication
- **Error Handling**: User-friendly error messages
- **Loading States**: Visual feedback during API calls
- **Cross-browser Compatibility**: Works in all modern browsers

## Getting Started

### Prerequisites
- .NET 9.0 SDK
- MongoDB running locally or remotely
- Modern web browser

### Backend Setup
1. Navigate to the backend directory:
   ```bash
   cd backend
   ```

2. Update MongoDB connection string in `appsettings.json`:
   ```json
   {
     "MongoDbSettings": {
       "ConnectionString": "mongodb://localhost:27017",
       "DatabaseName": "urlshortener"
     }
   }
   ```

3. Build and run the application:
   ```bash
   dotnet build
   dotnet run --project Urlshortener.Api
   ```

### Testing the Dashboard
1. Run the test script to create sample data:
   ```powershell
   .\test-dashboard.ps1
   ```

2. Open `dashboard.html` in your web browser

3. Login with the test credentials:
   - Username: `dashboarduser`
   - Password: `TestPass123`

### Using Postman
1. Import the `UrlShortener_API.postman_collection.json` file
2. Set the `baseUrl` environment variable to `http://localhost:8080`
3. Use the "Dashboard" collection to test all endpoints

## Data Models

### DashboardSummaryDto
```csharp
public class DashboardSummaryDto
{
    public int TotalLinks { get; set; }
    public int TotalClicks { get; set; }
    public int ClicksToday { get; set; }
    public int ClicksThisWeek { get; set; }
    public int ClicksThisMonth { get; set; }
    public List<LinkSummaryDto> TopLinks { get; set; }
    public List<DailyClicksDto> DailyClicks { get; set; }
}
```

### LinkSummaryDto
```csharp
public class LinkSummaryDto
{
    public string Id { get; set; }
    public string ShortCode { get; set; }
    public string OriginalUrl { get; set; }
    public string ShortUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalClicks { get; set; }
    public int ClicksToday { get; set; }
    public int ClicksThisWeek { get; set; }
    public int ClicksThisMonth { get; set; }
}
```

### LinkAnalyticsDto
```csharp
public class LinkAnalyticsDto
{
    public string Id { get; set; }
    public string ShortCode { get; set; }
    public string OriginalUrl { get; set; }
    public string ShortUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalClicks { get; set; }
    public List<ClickAnalytics> Clicks { get; set; }
    public List<DailyClicksDto> DailyClicks { get; set; }
}
```

## Security Features

- **JWT Authentication**: Secure token-based authentication
- **User Isolation**: Users can only access their own data
- **Input Validation**: All inputs are validated using FluentValidation
- **Rate Limiting**: API endpoints are protected against abuse
- **CORS Support**: Configured for cross-origin requests

## Performance Considerations

- **Efficient Queries**: MongoDB queries are optimized for performance
- **Caching**: Rate limiting uses in-memory caching
- **Indexing**: MongoDB collections are properly indexed
- **Pagination**: Large datasets can be paginated (future enhancement)

## Future Enhancements

- **Real-time Updates**: WebSocket support for live dashboard updates
- **Advanced Analytics**: Geographic data, device types, referral sources
- **Export Features**: CSV/PDF export of analytics data
- **Custom Domains**: Support for custom short URLs
- **QR Code Generation**: Automatic QR code generation for links
- **Bulk Operations**: Create and manage multiple links at once
- **Link Expiration**: Set expiration dates for links
- **Password Protection**: Password-protected short URLs

## Troubleshooting

### Common Issues

1. **CORS Errors**: Ensure the backend is running and accessible
2. **Authentication Failures**: Check JWT token validity and expiration
3. **Database Connection**: Verify MongoDB is running and accessible
4. **Port Conflicts**: Ensure port 8080 is available for the backend

### Debug Mode
Enable detailed logging by setting the environment:
```bash
set ASPNETCORE_ENVIRONMENT=Development
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## License

This project is licensed under the MIT License. 