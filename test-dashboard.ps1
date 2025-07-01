# Test Dashboard Endpoints
$baseUrl = "http://localhost:8080"
$authToken = ""

Write-Host "Testing URL Shortener Dashboard Endpoints" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green

# Step 1: Register a test user
Write-Host "`n1. Registering test user..." -ForegroundColor Yellow
$registerBody = @{
    username = "dashboarduser"
    password = "TestPass123"
} | ConvertTo-Json

try {
    $registerResponse = Invoke-RestMethod -Uri "$baseUrl/register" -Method POST -Body $registerBody -ContentType "application/json"
    Write-Host "✓ Registration successful" -ForegroundColor Green
} catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "ℹ User already exists, continuing..." -ForegroundColor Blue
    } else {
        Write-Host "✗ Registration failed: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
}

# Step 2: Login to get token
Write-Host "`n2. Logging in..." -ForegroundColor Yellow
$loginBody = @{
    username = "dashboarduser"
    password = "TestPass123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/login" -Method POST -Body $loginBody -ContentType "application/json"
    $authToken = $loginResponse.token
    Write-Host "✓ Login successful, token received" -ForegroundColor Green
} catch {
    Write-Host "✗ Login failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 3: Create some test URLs
Write-Host "`n3. Creating test URLs..." -ForegroundColor Yellow
$urls = @(
    "https://www.google.com",
    "https://www.github.com",
    "https://www.stackoverflow.com"
)

$shortCodes = @()
foreach ($url in $urls) {
    $shortenBody = @{
        originalUrl = $url
    } | ConvertTo-Json

    try {
        $shortenResponse = Invoke-RestMethod -Uri "$baseUrl/shorten" -Method POST -Body $shortenBody -ContentType "application/json" -Headers @{Authorization = "Bearer $authToken"}
        $shortCode = $shortenResponse.shortUrl.Split('/')[-1]
        $shortCodes += $shortCode
        Write-Host "✓ Created short URL: $shortCode" -ForegroundColor Green
    } catch {
        Write-Host "✗ Failed to create short URL: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Step 4: Generate some clicks for analytics
Write-Host "`n4. Generating test clicks..." -ForegroundColor Yellow
foreach ($shortCode in $shortCodes) {
    try {
        # Simulate clicks by accessing the redirect URL
        $redirectResponse = Invoke-WebRequest -Uri "$baseUrl/r/$shortCode" -Method GET -MaximumRedirection 0 -ErrorAction SilentlyContinue
        Write-Host "✓ Generated click for: $shortCode" -ForegroundColor Green
    } catch {
        # Expected to fail due to redirect, but click should be recorded
        Write-Host "✓ Generated click for: $shortCode (redirect expected)" -ForegroundColor Green
    }
}

# Step 5: Test Dashboard Summary
Write-Host "`n5. Testing Dashboard Summary..." -ForegroundColor Yellow
try {
    $dashboardResponse = Invoke-RestMethod -Uri "$baseUrl/dashboard" -Method GET -Headers @{Authorization = "Bearer $authToken"}
    Write-Host "✓ Dashboard summary retrieved successfully" -ForegroundColor Green
    Write-Host "  - Total Links: $($dashboardResponse.totalLinks)" -ForegroundColor Cyan
    Write-Host "  - Total Clicks: $($dashboardResponse.totalClicks)" -ForegroundColor Cyan
    Write-Host "  - Clicks Today: $($dashboardResponse.clicksToday)" -ForegroundColor Cyan
    Write-Host "  - Clicks This Week: $($dashboardResponse.clicksThisWeek)" -ForegroundColor Cyan
    Write-Host "  - Top Links Count: $($dashboardResponse.topLinks.Count)" -ForegroundColor Cyan
} catch {
    Write-Host "✗ Dashboard summary failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 6: Test User Links
Write-Host "`n6. Testing User Links..." -ForegroundColor Yellow
try {
    $linksResponse = Invoke-RestMethod -Uri "$baseUrl/dashboard/links" -Method GET -Headers @{Authorization = "Bearer $authToken"}
    Write-Host "✓ User links retrieved successfully" -ForegroundColor Green
    Write-Host "  - Total Links: $($linksResponse.Count)" -ForegroundColor Cyan
    
    foreach ($link in $linksResponse) {
        Write-Host "  - $($link.shortCode): $($link.totalClicks) clicks" -ForegroundColor Cyan
    }
} catch {
    Write-Host "✗ User links failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 7: Test Individual Link Analytics
Write-Host "`n7. Testing Individual Link Analytics..." -ForegroundColor Yellow
if ($shortCodes.Count -gt 0) {
    $testShortCode = $shortCodes[0]
    try {
        $analyticsResponse = Invoke-RestMethod -Uri "$baseUrl/dashboard/links/$testShortCode" -Method GET -Headers @{Authorization = "Bearer $authToken"}
        Write-Host "✓ Link analytics retrieved successfully for: $testShortCode" -ForegroundColor Green
        Write-Host "  - Total Clicks: $($analyticsResponse.totalClicks)" -ForegroundColor Cyan
        Write-Host "  - Individual Clicks Count: $($analyticsResponse.clicks.Count)" -ForegroundColor Cyan
        Write-Host "  - Daily Clicks Count: $($analyticsResponse.dailyClicks.Count)" -ForegroundColor Cyan
    } catch {
        Write-Host "✗ Link analytics failed: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`n✅ Dashboard testing completed!" -ForegroundColor Green
Write-Host "You can now open dashboard.html in your browser to see the frontend." -ForegroundColor Cyan 