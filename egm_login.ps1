try {
  $body = @{ Sicil = 425394; Password = 'S5s5mr.kfks' }
  $res = Invoke-RestMethod -Uri 'http://localhost:5117/api/auth/login' -Method Post -Body ($body | ConvertTo-Json) -ContentType 'application/json' -TimeoutSec 10
  Write-Host 'LOGIN_OK'
  Write-Host 'TOKEN:' $res.Token
} catch {
  Write-Host 'LOGIN_ERROR:' $_.Exception.Message
}
