# URL Shortener API Documentation

## Overview

The URL Shortener API is a RESTful service built with ASP.NET Core that provides URL shortening functionality with comprehensive analytics and user management.

**Base URL:** `http://localhost:5084`  
**Content-Type:** `application/json`  
**Authentication:** JWT Bearer Token

## Authentication

All protected endpoints require a valid JWT token in the Authorization header:

```
Authorization: Bearer <your_jwt_token>
```

## Endpoints

### Authentication

#### Register User
**POST** `/register`

Creates a new user account.

**Request Body:**
```json
{
  "username": "string",
  "password": "string"
}
```

**Validation Rules:**
- Username: 3-50 characters, alphanumeric and underscores only
- Password: Minimum 6 characters

**Response:**
```json
{
  "message": "Registration successful."
}
```

**Error Responses:**
- `400 Bad Request`: Invalid input or username already exists
- `500 Internal Server Error`: Server error

**Example:**
```bash
curl -X POST http://localhost:5084/register \
  -H "Content-Type: application/json" \
  -d '{"username": "newuser", "password": "SecurePass123"}'
```

#### Login User
**POST** `/login`

Authenticates user and returns JWT token.

**Request Body:**
```json
{
  "username": "string",
  "password": "string"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Error Responses:**
- `400 Bad Request`: Invalid credentials
- `500 Internal Server Error`: Server error

**Example:**
```bash
curl -X POST http://localhost:5084/login \
  -H "Content-Type: application/json" \
  -d '{"username": "newuser", "password": "SecurePass123"}'
```

#### Get Current User
**GET** `/me`

Returns information about the currently authenticated user.

**Headers:**
```
Authorization: Bearer <token>
```

**Response:**
```json
{
  "id": "507f1f77bcf86cd799439011",
  "username": "newuser"
}
```

**Error Responses:**
- `401 Unauthorized`: Invalid or missing token

**Example:**
```bash
curl -X GET http://localhost:5084/me \
  -H "Authorization: Bearer <your_token>"
```

### URL Management

#### Create Short URL
**POST** `/shorten`

Creates a new shortened URL for the authenticated user.

**Headers:**
```
Authorization: Bearer <token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "originalUrl": "string"
}
```

**Validation Rules:**
- OriginalUrl: Must be a valid URL

**Response:**
```json
{
  "shortUrl": "http://localhost:5084/r/abc123"
}
```

**Error Responses:**
- `400 Bad Request`: Invalid URL
- `401 Unauthorized`: Invalid or missing token

**Example:**
```bash
curl -X POST http://localhost:5084/shorten \
  -H "Authorization: Bearer <your_token>" \
  -H "Content-Type: application/json" \
  -d '{"originalUrl": "https://www.example.com/very/long/url"}'
```

#### Redirect to Original URL
**GET** `/r/{shortCode}`

Redirects to the original URL and tracks analytics.

**Parameters:**
- `shortCode` (string): The short code of the URL

**Response:**
- `302 Found`: Redirects to original URL
- `404 Not Found`: Short URL not found

**Example:**
```bash
curl -L http://localhost:5084/r/abc123
```

### Analytics

#### Get Dashboard Summary
**GET** `/dashboard`

Returns comprehensive analytics summary for the authenticated user.

**Headers:**
```
Authorization: Bearer <token>
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

**Error Responses:**
- `401 Unauthorized`: Invalid or missing token

**Example:**
```bash
curl -X GET http://localhost:5084/dashboard \
  -H "Authorization: Bearer <your_token>"
```

#### Get User Links
**GET** `/dashboard/links`

Returns all links created by the authenticated user with analytics.

**Headers:**
```
Authorization: Bearer <token>
```

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

**Error Responses:**
- `401 Unauthorized`: Invalid or missing token

**Example:**
```bash
curl -X GET http://localhost:5084/dashboard/links \
  -H "Authorization: Bearer <your_token>"
```

#### Get Link Analytics
**GET** `/dashboard/links/{shortCode}`

Returns detailed analytics for a specific link.

**Headers:**
```
Authorization: Bearer <token>
```

**Parameters:**
- `shortCode` (string): The short code of the URL

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
      "userAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"
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

**Error Responses:**
- `400 Bad Request`: Invalid short code
- `401 Unauthorized`: Invalid or missing token
- `404 Not Found`: Link not found or not owned by user

**Example:**
```bash
curl -X GET http://localhost:5084/dashboard/links/abc123 \
  -H "Authorization: Bearer <your_token>"
```

#### Get Raw Analytics
**GET** `/analytics/{shortCode}`

Returns raw click analytics for a specific link.

**Headers:**
```
Authorization: Bearer <token>
```

**Parameters:**
- `shortCode` (string): The short code of the URL

**Response:**
```json
[
  {
    "id": "507f1f77bcf86cd799439012",
    "shortUrlId": "507f1f77bcf86cd799439011",
    "timestamp": "2024-01-15T14:30:00Z",
    "ipAddress": "192.168.1.1",
    "userAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"
  }
]
```

**Error Responses:**
- `400 Bad Request`: Invalid short code
- `401 Unauthorized`: Invalid or missing token
- `404 Not Found`: Link not found or not owned by user

**Example:**
```bash
curl -X GET http://localhost:5084/analytics/abc123 \
  -H "Authorization: Bearer <your_token>"
```

### System

#### Health Check
**GET** `/health`

Returns the health status of the API.

**Response:**
```json
{
  "status": "healthy",
  "timestamp": "2024-01-15T14:30:00Z"
}
```

**Example:**
```bash
curl -X GET http://localhost:5084/health
```

## Data Models

### User
```json
{
  "id": "string",
  "username": "string",
  "passwordHash": "string"
}
```

### ShortUrl
```json
{
  "id": "string",
  "originalUrl": "string",
  "shortCode": "string",
  "userId": "string",
  "createdAt": "datetime"
}
```

### ClickAnalytics
```json
{
  "id": "string",
  "shortUrlId": "string",
  "timestamp": "datetime",
  "ipAddress": "string",
  "userAgent": "string"
}
```

### DashboardSummaryDto
```json
{
  "totalLinks": "number",
  "totalClicks": "number",
  "clicksToday": "number",
  "clicksThisWeek": "number",
  "clicksThisMonth": "number",
  "topLinks": "LinkSummaryDto[]",
  "dailyClicks": "DailyClicksDto[]"
}
```

### LinkSummaryDto
```json
{
  "id": "string",
  "shortCode": "string",
  "originalUrl": "string",
  "shortUrl": "string",
  "createdAt": "datetime",
  "totalClicks": "number",
  "clicksToday": "number",
  "clicksThisWeek": "number",
  "clicksThisMonth": "number"
}
```

### LinkAnalyticsDto
```json
{
  "id": "string",
  "shortCode": "string",
  "originalUrl": "string",
  "shortUrl": "string",
  "createdAt": "datetime",
  "totalClicks": "number",
  "clicks": "ClickAnalytics[]",
  "dailyClicks": "DailyClicksDto[]"
}
```

### DailyClicksDto
```json
{
  "date": "datetime",
  "clicks": "number"
}
```

## Error Handling

### Error Response Format
```json
{
  "message": "Error description",
  "errors": [
    "Detailed error message 1",
    "Detailed error message 2"
  ]
}
```

### HTTP Status Codes

| Code | Description |
|------|-------------|
| 200 | OK - Request successful |
| 201 | Created - Resource created successfully |
| 302 | Found - Redirect (for short URLs) |
| 400 | Bad Request - Invalid input |
| 401 | Unauthorized - Authentication required |
| 404 | Not Found - Resource not found |
| 429 | Too Many Requests - Rate limit exceeded |
| 500 | Internal Server Error - Server error |

### Common Error Messages

| Error | Description | Solution |
|-------|-------------|----------|
| `Username already exists` | Registration with existing username | Use different username |
| `Invalid username or password` | Login with wrong credentials | Check credentials |
| `Invalid URL` | URL format is incorrect | Ensure URL starts with http:// or https:// |
| `Short URL not found` | Short code doesn't exist | Verify short code |
| `Short URL not found or not owned by user` | Accessing another user's link | Use your own short code |

## Rate Limiting

The API implements rate limiting to prevent abuse:

- **Limit:** 100 requests per minute per IP address
- **Headers:** Rate limit information is included in response headers
  - `X-Rate-Limit-Limit`: Request limit per period
  - `X-Rate-Limit-Remaining`: Remaining requests in current period
  - `X-Rate-Limit-Reset`: Time when the rate limit resets

**Example Response Headers:**
```
X-Rate-Limit-Limit: 1m
X-Rate-Limit-Remaining: 95
X-Rate-Limit-Reset: 2024-01-15T14:31:00Z
```

## Authentication Flow

1. **Register** a new user account
2. **Login** to receive JWT token
3. **Include token** in Authorization header for protected endpoints
4. **Token expires** after 7 days (requires re-login)

## Testing

### Using Postman
1. Import the provided Postman collection
2. Set environment variable `baseUrl` to `http://localhost:5084`
3. Run the collection to test all endpoints

### Using curl
Examples are provided for each endpoint above.

### Using PowerShell
Run the test script:
```powershell
.\test-dashboard.ps1
```

## Security Considerations

- JWT tokens are valid for 7 days
- Passwords are hashed using BCrypt
- Rate limiting prevents abuse
- CORS is configured for cross-origin requests
- Input validation prevents malicious data
- Users can only access their own data

## Performance Notes

- MongoDB queries are optimized for performance
- Rate limiting uses in-memory caching
- Async operations prevent blocking
- Connection pooling is enabled for MongoDB

## Support

For API support:
1. Check the error messages and status codes
2. Verify your authentication token is valid
3. Ensure you're using the correct base URL
4. Check rate limiting headers if getting 429 errors 