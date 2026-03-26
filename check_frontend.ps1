try {
  $r = Invoke-WebRequest -Uri 'http://localhost:4200' -UseBasicParsing -TimeoutSec 5
  Write-Host 'FRONTEND_OK'
  Write-Host ('Status:' + $r.StatusCode)
} catch {
  Write-Host 'FRONTEND_ERROR:' $_.Exception.Message
}
