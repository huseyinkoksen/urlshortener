# URL Shortener API Test Script
$baseUrl = "http://localhost:8080"
$token = ""

Write-Host "=== URL Shortener API Testing ===" -ForegroundColor Green

# Test 1: Register User
Write-Host "`n1. Testing User Registration..." -ForegroundColor Yellow
try {
    $registerBody = @{
        username = "testuser"
        password = "TestPass123"
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod -Uri "$baseUrl/register" -Method POST -Headers @{"Content-Type"="application/json"} -Body $registerBody
    Write-Host "✅ Registration successful: $($response.message)" -ForegroundColor Green
} catch {
    Write-Host "❌ Registration failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Login User
Write-Host "`n2. Testing User Login..." -ForegroundColor Yellow
try {
    $loginBody = @{
        username = "testuser"
        password = "TestPass123"
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod -Uri "$baseUrl/login" -Method POST -Headers @{"Content-Type"="application/json"} -Body $loginBody
    $token = $response.token
    Write-Host "✅ Login successful: Token received" -ForegroundColor Green
    Write-Host "Token: $($token.Substring(0, 20))..." -ForegroundColor Gray
} catch {
    Write-Host "❌ Login failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Get Current User
Write-Host "`n3. Testing Get Current User..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/me" -Method GET -Headers @{"Authorization"="Bearer $token"}
    Write-Host "✅ Get user successful: $($response.username)" -ForegroundColor Green
} catch {
    Write-Host "❌ Get user failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: Create Short URL
Write-Host "`n4. Testing Create Short URL..." -ForegroundColor Yellow
try {
    $shortenBody = @{
        originalUrl = "https://www.google.com"
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod -Uri "$baseUrl/shorten" -Method POST -Headers @{"Content-Type"="application/json"; "Authorization"="Bearer $token"} -Body $shortenBody
    $shortUrl = $response.shortUrl
    $shortCode = $shortUrl.Split('/')[-1]
    Write-Host "✅ Short URL created: $shortUrl" -ForegroundColor Green
    Write-Host "Short Code: $shortCode" -ForegroundColor Gray
} catch {
    Write-Host "❌ Create short URL failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 5: Test Redirect (should return 302)
Write-Host "`n5. Testing Redirect..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/r/$shortCode" -Method GET -MaximumRedirection 0
    Write-Host "✅ Redirect successful: Status $($response.StatusCode)" -ForegroundColor Green
    Write-Host "Redirect URL: $($response.Headers.Location)" -ForegroundColor Gray
} catch {
    if ($_.Exception.Response.StatusCode -eq 302) {
        Write-Host "✅ Redirect successful: Status 302" -ForegroundColor Green
    } else {
        Write-Host "❌ Redirect failed: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Test 6: View Analytics
Write-Host "`n6. Testing View Analytics..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/analytics/$shortCode" -Method GET -Headers @{"Authorization"="Bearer $token"}
    Write-Host "✅ Analytics retrieved: $($response.Count) click(s)" -ForegroundColor Green
} catch {
    Write-Host "❌ Analytics failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== Testing Complete ===" -ForegroundColor Green 