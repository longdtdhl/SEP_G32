$baseUrl = "http://localhost:5152/api/v1"

Write-Host "Testing Admin Login..."
$loginBody = @{
    email = "admin@opcbs.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
    if ($loginResponse.success) {
        Write-Host "Login Successful!"
        $token = $loginResponse.data.accessToken
        Write-Host "Token: $token"
        
        Write-Host "`nTesting GET /users/profile..."
        $headers = @{
            Authorization = "Bearer $token"
        }
        
        $profileResponse = Invoke-RestMethod -Uri "$baseUrl/users/profile" -Method Get -Headers $headers
        $profileResponse | ConvertTo-Json -Depth 5
    } else {
        Write-Host "Login Failed (Response):"
        $loginResponse | ConvertTo-Json -Depth 5
    }
} catch {
    Write-Host "Error occurred:"
    $_.Exception.Message
    if ($_.Exception.Response) {
        $stream = $_.Exception.Response.GetResponseStream()
        if ($stream) {
            $reader = New-Object System.IO.StreamReader($stream)
            $reader.ReadToEnd()
        }
    }
}
